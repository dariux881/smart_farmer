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
    private FarmerPoint _toolsCollectorPosition;
    private SemaphoreSlim _mountingToolSem;
    private IFarmerMoveOnGridTask _moveOnGrid;
    private IFarmerMoveArmAtHeight _moveHeight;

    private IFarmerTaskProvider _taskProvider = FarmerServiceLocator.GetService<IFarmerTaskProvider>(true);

    public FarmerToolsManager()
    {
        _currentlyMountedTool = FarmerTool.None;
        _mountingToolSem = new SemaphoreSlim(1);

        InitializeDependencies();
    }

    public void SetToolCollectorPosition(FarmerPoint toolsCollectorPosition)
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
            _taskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveOnGridTask).FullName)
                    as IFarmerMoveOnGridTask;

        _moveHeight =
            _taskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveArmAtHeight).FullName)
                    as IFarmerMoveArmAtHeight;
    }
}
