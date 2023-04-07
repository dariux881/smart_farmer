using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Helpers;

public class FarmerToolsManager : IFarmerToolsManager
{
    private FarmerTool _currentlyMountedTool;
    private Farmer2dPoint _toolsCollectorPosition;
    private IFarmerGround _ground;
    private SemaphoreSlim _mountingToolSem;
    private IFarmerMoveOnGridTask _moveOnGrid;
    private IFarmerMoveArmAtHeight _moveHeight;

    public FarmerToolsManager(IFarmerGround ground)
    {
        _currentlyMountedTool = FarmerTool.None;
        _mountingToolSem = new SemaphoreSlim(1);

        _ground = ground;

        InitializeDependencies();
    }

    public void SetToolCollectorPosition(Farmer2dPoint toolsCollectorPosition)
    {
        _toolsCollectorPosition = toolsCollectorPosition;
    }

    public FarmerTool GetCurrentlyMountedTool()
    {
        _mountingToolSem.Wait();

        var tool = _currentlyMountedTool;

        _mountingToolSem.Release();

        return tool;
    }

    public async Task MountTool(FarmerTool tool, CancellationToken token)
    {
        var currentTool = GetCurrentlyMountedTool();
        if (currentTool == tool || tool == FarmerTool.None)
        {
            await Task.CompletedTask;
            return;
        }

        _mountingToolSem.Wait();

        SmartFarmerLog.Debug($"Mounting tool {tool}");

        _moveOnGrid.GetCurrentPosition(out var x, out var y);

        SmartFarmerLog.Debug($"Moving to tool positions");

        if (_toolsCollectorPosition == null)
        {
            throw new InvalidOperationException("unknown position for tools");
        }

        await _moveOnGrid.MoveToPosition(_toolsCollectorPosition.X, _toolsCollectorPosition.Y, token);

        //TODO mount tool
        //TODO raise exception in case of mounting failure

        _currentlyMountedTool = tool;
        SmartFarmerLog.Debug($"Tool {tool} mounted");

        SmartFarmerLog.Debug($"Returning to original position");
        await _moveOnGrid.MoveToPosition(x, y, token);

        _mountingToolSem.Release();
        await Task.CompletedTask;
    }

    private void InitializeDependencies()
    {
        _moveOnGrid =
            FarmerServiceLocator
                .GetService<IFarmerMoveOnGridTask>(true, _ground);

        _moveHeight =
            FarmerServiceLocator
                .GetService<IFarmerMoveArmAtHeight>(true, _ground);
    }
}
