using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Handlers;

public class FarmerLocalInformationManager : IFarmerLocalInformationManager
{
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;
    private IFarmerDeviceKindProvider _deviceProvider;

    public FarmerLocalInformationManager()
    {
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
        FarmerServiceLocator.RemoveService<IFarmerProvideWaterTask>(garden);
        FarmerServiceLocator.RemoveService<FarmerProvideWaterTask>(garden);

        // preparing new services
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(garden), garden);

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

        var provideWaterTask = new FarmerProvideWaterTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerProvideWaterTask>(() => provideWaterTask, garden);
        FarmerServiceLocator.MapService<FarmerProvideWaterTask>(() => provideWaterTask, garden);

        await moveOnGridTask.InitializeAsync(cancellationToken);
        await moveAtHeightTask.InitializeAsync(cancellationToken);
        await alertHandler.InitializeAsync(cancellationToken);

        SmartFarmerLog.Debug("Services added");
    }
}