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
        Grounds = new ConcurrentDictionary<string, IFarmerGround>();

        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
        _deviceProvider = FarmerServiceLocator.GetService<IFarmerDeviceKindProvider>(true);
    }

    public ConcurrentDictionary<string, IFarmerGround> Grounds { get; }

    public async Task ReinitializeGroundsAsync(CancellationToken token)
    {
        ClearLocalData(true, false, false);
        await InitializeGroundsAsync(token);
    }

    public async Task InitializeGroundsAsync(CancellationToken token)
    {
        // get all grounds
        var grounds = await FarmerRequestHandler.GetGroundsList(token);
        if (grounds == null)
        {
            SmartFarmerLog.Error("invalid grounds");
            return;
        }

        if (!grounds.Any())
        {
            SmartFarmerLog.Debug("no assigned grounds");
            return;
        }

        SmartFarmerLog.Debug(
                "List of grounds:\n\t" + grounds.Select(x => x.ID).Aggregate((g1, g2) => g1 + ", " + g2));

        var locallyInterestedGrounds = 
            _configProvider.GetAppConfiguration().LocalGroundIds != null && _configProvider.GetAppConfiguration().LocalGroundIds.Any() ?
                grounds
                    .Where(x => 
                        _configProvider.GetAppConfiguration().LocalGroundIds.Contains(x.ID)).ToList() :
                new List<IFarmerGround>() { grounds.First() };

        var tasks = new List<Task>();

        foreach (var ground in locallyInterestedGrounds)
        {
            await InitializeServicesForSingleGround(ground, token);

            var groundId = ground.ID;
            tasks.Add(Task.Run(async () => {
                var ground = await FarmerRequestHandler
                    .GetGround(groundId, token);
                
                Grounds.TryAdd(ground.ID, ground);
                _communicationHandler.NotifyNewGround(ground.ID);
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
        bool clearGrounds = false,
        bool clearLoggedUser = false,
        bool clearToken = false)
    {
        if (clearGrounds)
        {
            foreach (var ground in Grounds.Values)
            {
                if (ground is IDisposable disp)
                {
                    disp.Dispose();
                }
            }

            Grounds.Clear();
        }
    }

    private async Task InitializeServicesForSingleGround(IFarmerGround ground, CancellationToken cancellationToken)
    {
        // clearing possibly old mapped services
        FarmerServiceLocator.RemoveService<IFarmerToolsManager>();
        FarmerServiceLocator.RemoveService<IFarmerAlertHandler>();
        FarmerServiceLocator.RemoveService<IFarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveOnGridTask>();
        FarmerServiceLocator.RemoveService<IFarmerMoveArmAtHeightTask>();
        FarmerServiceLocator.RemoveService<FarmerMoveArmAtHeightTask>();
        FarmerServiceLocator.RemoveService<IFarmerProvideWaterTask>();
        FarmerServiceLocator.RemoveService<FarmerProvideWaterTask>();

        // preparing new services
        FarmerServiceLocator.MapService<IFarmerToolsManager>(() => new FarmerToolsManager(ground));

        var alertHandler = new FarmerAlertHandler(ground, _configProvider.GetHubConfiguration());
        FarmerServiceLocator.MapService<IFarmerAlertHandler>(() => alertHandler, ground);

        var deviceHandler = _deviceProvider.GetDeviceManager(ground.ID);

        var moveOnGridTask = new FarmerMoveOnGridTask(ground, deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveOnGridTask>(() => moveOnGridTask, ground);
        FarmerServiceLocator.MapService<FarmerMoveOnGridTask>(() => moveOnGridTask, ground);

        var moveAtHeightTask = new FarmerMoveArmAtHeightTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerMoveArmAtHeightTask>(() => moveAtHeightTask, ground);
        FarmerServiceLocator.MapService<FarmerMoveArmAtHeightTask>(() => moveAtHeightTask, ground);

        var provideWaterTask = new FarmerProvideWaterTask(deviceHandler);
        FarmerServiceLocator.MapService<IFarmerProvideWaterTask>(() => provideWaterTask, ground);
        FarmerServiceLocator.MapService<FarmerProvideWaterTask>(() => provideWaterTask, ground);

        await moveOnGridTask.Initialize(cancellationToken);
        await moveAtHeightTask.Initialize(cancellationToken);
        await alertHandler.InitializeAsync(cancellationToken);
    }
}