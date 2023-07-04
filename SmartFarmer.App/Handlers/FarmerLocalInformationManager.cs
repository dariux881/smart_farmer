using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Detection;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerLocalInformationManager : IFarmerLocalInformationManager
{
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;
    private IFarmerDeviceKindProvider _deviceProvider;
    private ConcurrentDictionary<string, object> _volatileDictionary;

    public FarmerLocalInformationManager()
    {
        _volatileDictionary = new ConcurrentDictionary<string, object>();
        Gardens = new ConcurrentDictionary<string, IFarmerGarden>();

        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
        _deviceProvider = FarmerServiceLocator.GetService<IFarmerDeviceKindProvider>(true);
    }

    public ConcurrentDictionary<string, IFarmerGarden> Gardens { get; }

    public async Task ReinitializeGardensAsync(CancellationToken token)
    {
        ClearLocalData(true, false, false);
        await InitializeGardensAsync(token);
    }

    public async Task InitializeGardensAsync(CancellationToken token)
    {
        // get all gardens
        var gardens = await FarmerRequestHandler.GetGardensList(token);
        if (gardens == null)
        {
            SmartFarmerLog.Error("invalid gardens");
            return;
        }

        if (!gardens.Any())
        {
            SmartFarmerLog.Debug("no assigned gardens");
            return;
        }

        SmartFarmerLog.Debug(
                "List of gardens:\n\t" + gardens.Select(x => x.ID).Aggregate((g1, g2) => g1 + ", " + g2));

        var locallyInterestedGardens = 
            _configProvider.GetAppConfiguration().LocalGardenIds != null && _configProvider.GetAppConfiguration().LocalGardenIds.Any() ?
                gardens
                    .Where(x => 
                        _configProvider.GetAppConfiguration().LocalGardenIds.Contains(x.ID)).ToList() :
                new List<IFarmerGarden>() { gardens.First() };

        var tasks = new List<Task>();

        foreach (var garden in locallyInterestedGardens)
        {
            var gardenId = garden.ID;
            tasks.Add(Task.Run(async () => {
                var garden = await FarmerRequestHandler
                    .GetGarden(gardenId, token);
                
                await InitializeServicesForSingleGarden(garden, token).ConfigureAwait(true);

                Gardens.TryAdd(garden.ID, garden);

                SmartFarmerLog.Debug("Notifying new garden");
                _communicationHandler.NotifyNewGarden(garden.ID);
            }));
        }

        try {  
           await Task.WhenAll(tasks);
        }  
        catch(AggregateException ae) 
        {
            SmartFarmerLog.Exception(ae);
        }
        catch(Exception ex) 
        {
            SmartFarmerLog.Exception(ex);
        }
    }

    public void ClearLocalData(
        bool clearGardens = false,
        bool clearLoggedUser = false,
        bool clearToken = false)
    {
        if (clearGardens)
        {
            foreach (var garden in Gardens.Values)
            {
                if (garden is IDisposable disp)
                {
                    disp.Dispose();
                }
            }

            Gardens.Clear();
        }
    }

    public void PushVolatileData(string key, object data)
    {
        _volatileDictionary.TryAdd(key, data);
    }

    public object PickVolatileData(string key)
    {
        if (_volatileDictionary.TryGetValue(key, out var data))
        {
            return data;
        }

        return null;
    }

    private async Task InitializeServicesForSingleGarden(IFarmerGarden garden, CancellationToken cancellationToken)
    {
        // clearing possibly old mapped services
        FarmerServiceLocator.RemoveService<IFarmerToolsManager>(garden);
        FarmerServiceLocator.RemoveService<IFarmerAlertHandler>(garden);
        FarmerServiceLocator.RemoveService<FarmerAlertHandler>(garden);
        FarmerServiceLocator.RemoveService<IFarmerMoveOnGridTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerMoveOnGridTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerMoveArmAtHeightTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerMoveArmAtHeightTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerMoveArmAtMaxHeightTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerMoveArmAtMaxHeightTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerProvideWaterTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerProvideWaterTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerTakePictureTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerTakePictureTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerPointTargetTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerPointTargetTask>(garden);
        FarmerServiceLocator.RemoveService<IFarmerTurnArmToDegreeTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerTurnArmToDegreeTask>(garden);

        // preparing new services
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(garden), garden);

        var pictureTask = new FarmerTakePictureTask(_configProvider.GetGardenConfiguration(garden.ID)?.CameraConfiguration);
        FarmerServiceLocator.MapService<IFarmerTakePictureTask>(() => pictureTask, garden);
        FarmerServiceLocator.MapService<FarmerTakePictureTask>(() => pictureTask, garden);

        var alertHandler = new FarmerAlertHandler(garden, _configProvider.GetHubConfiguration());
        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => alertHandler, garden);
        FarmerServiceLocator.MapService<FarmerAlertHandler>(() => alertHandler, garden);

        var deviceHandler = _deviceProvider.GetDeviceManager(garden.ID);

        var moveOnGridTask = new FarmerMoveOnGridTask(garden, deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveOnGridTask>(() => moveOnGridTask, garden);
        FarmerServiceLocator.MapService<FarmerMoveOnGridTask>(() => moveOnGridTask, garden);

        var moveAtHeightTask = new FarmerMoveArmAtHeightTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtHeightTask>(() => moveAtHeightTask, garden);
        FarmerServiceLocator.MapService<FarmerMoveArmAtHeightTask>(() => moveAtHeightTask, garden);

        var moveAtMaxHeightTask = new FarmerMoveArmAtMaxHeightTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtMaxHeightTask>(() => moveAtMaxHeightTask, garden);
        FarmerServiceLocator.MapService<FarmerMoveArmAtMaxHeightTask>(() => moveAtMaxHeightTask, garden);

        var provideWaterTask = new FarmerProvideWaterTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerProvideWaterTask>(() => provideWaterTask, garden);
        FarmerServiceLocator.MapService<FarmerProvideWaterTask>(() => provideWaterTask, garden);

        var pointTargetTask = new FarmerPointTargetTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerPointTargetTask>(() => pointTargetTask, garden);
        FarmerServiceLocator.MapService<FarmerPointTargetTask>(() => pointTargetTask, garden);

        var turnArmTask = new FarmerTurnArmToDegreeTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerTurnArmToDegreeTask>(() => turnArmTask, garden);
        FarmerServiceLocator.MapService<FarmerTurnArmToDegreeTask>(() => turnArmTask, garden);

        await moveOnGridTask.InitializeAsync(cancellationToken);
        await moveAtHeightTask.InitializeAsync(cancellationToken);
        await alertHandler.InitializeAsync(cancellationToken);

        SmartFarmerLog.Debug("Services added");
    }
}