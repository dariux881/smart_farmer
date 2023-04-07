using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Settings;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Utils;

public class FarmerToolsManager
{
    private static readonly Lazy<FarmerToolsManager> _instance = new(() => new FarmerToolsManager());
    public static FarmerToolsManager Instance => _instance.Value;

    private FarmerTool _currentlyMountedTool;
    private Farmer2dPoint _toolsCollectorPosition;
    private SemaphoreSlim _mountingToolSem;
    private IFarmerMoveOnGridTask _moveOnGrid;
    private IFarmerMoveArmAtHeight _moveHeight;

    private FarmerToolsManager()
    {

    }

    public FarmerToolsManager(Farmer2dPoint toolsCollectorPosition)
    {
        _currentlyMountedTool = FarmerTool.None;
        _mountingToolSem = new SemaphoreSlim(1);

        _toolsCollectorPosition = 
            UserDefinedSettingsProvider
                .GetUserDefinedSettings(Configuration.LocalUserId)
                .TOOLS_COLLECTOR_POSITION;

        InitializeDependencies();
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

        if (_toolsCollectorPosition != null)
        {
            await _moveOnGrid.MoveToPosition(_toolsCollectorPosition.X, _toolsCollectorPosition.Y, token);
        }

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
            FarmerTaskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveOnGridTask).FullName)
                    as IFarmerMoveOnGridTask;
        
        _moveHeight =
            FarmerTaskProvider
                .GetTaskDelegateByInterfaceFullName(
                    typeof(IFarmerMoveArmAtHeight).FullName)
                    as IFarmerMoveArmAtHeight;
    }
}
