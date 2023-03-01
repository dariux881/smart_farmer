using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Base;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public class FarmerToolManager : FarmerBaseTask, IFarmerToolManager
{
    private FarmerTool _currentlyMountedTool;
    private SemaphoreSlim _mountingToolSem;
    private IFarmerMoveOnGridTask _moveOnGrid;
    private IFarmerMoveArmAtHeight _moveHeight;

    public FarmerToolManager()
    {
        _currentlyMountedTool = FarmerTool.None;
        _mountingToolSem = new SemaphoreSlim(1);

        InitializeDependencies();
    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || !parameters.Any())
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("invalid parameters");
        }

        if (!Enum.TryParse<FarmerTool>(parameters[0] as string, true, out var tool))
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("invalid tool");
        }

        await MountTool(tool, token);
    }

    public FarmerTool GetMountedTool()
    {
        _mountingToolSem.Wait();

        var tool = _currentlyMountedTool;

        _mountingToolSem.Release();

        return tool;
    }

    public async Task MountTool(FarmerTool tool, CancellationToken token)
    {
        var currentTool = GetMountedTool();
        if (currentTool == tool || tool == FarmerTool.None)
        {
            await Task.CompletedTask;
            return;
        }

        _mountingToolSem.Wait();
        
        SmartFarmerLog.Debug($"Mounting tool {tool}");

        _moveOnGrid.GetCurrentPosition(out var x, out var y);

        SmartFarmerLog.Debug($"Moving to tool positions");
        //TODO set tool position. Now 0, 0
        await _moveOnGrid.MoveToPosition(0, 0, token);

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
            FarmerDiscoveredTaskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveOnGridTask).FullName)
                    as IFarmerMoveOnGridTask;
        
        _moveHeight =
            FarmerDiscoveredTaskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveArmAtHeight).FullName)
                    as IFarmerMoveArmAtHeight;
    }
}