using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Misc;
using SmartFarmer.Position;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerToolsManager : IFarmerToolsManager
{
    private FarmerTool _currentlyMountedTool;
    private Farmer2dPoint _toolsCollectorPosition;
    private IFarmerGarden _garden;
    private SemaphoreSlim _mountingToolSem;
    private IFarmerMoveOnGridTask _moveOnGrid;
    private IFarmerMoveArmAtHeightTask _moveHeight;

    public FarmerToolsManager(IFarmerGarden garden)
    {
        _currentlyMountedTool = FarmerTool.None;
        _mountingToolSem = new SemaphoreSlim(1);

        _garden = garden;

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
                .GetService<IFarmerMoveOnGridTask>(true, _garden);

        _moveHeight =
            FarmerServiceLocator
                .GetService<IFarmerMoveArmAtHeightTask>(true, _garden);
    }
}
