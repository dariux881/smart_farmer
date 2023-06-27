using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Handlers;
using SmartFarmer.Handlers.Providers;
using SmartFarmer.Misc;
using SmartFarmer.OperationalManagement;

namespace SmartFarmer;

public class GardenActivityManager
{
    private List<IOperationalModeManager> _operationalManagers;
    private OperationalManagerRequestExecutor _requestExecutor;
    private readonly IFarmerAppCommunicationHandler _communicationHandler;
    private readonly IFarmerConfigurationProvider _configProvider;
    private readonly IFarmerLocalInformationManager _localInfoManager;
    private readonly IFarmerSessionManager _sessionManager;
    private CancellationTokenSource _managementTokenSource;

    public GardenActivityManager()
    {
        _requestExecutor = new OperationalManagerRequestExecutor();
        _managementTokenSource = new CancellationTokenSource();

        _configProvider = FarmerServiceLocator.GetService<IFarmerConfigurationProvider>(true);
        _communicationHandler = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);
        _localInfoManager = FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true);
        _sessionManager = FarmerServiceLocator.GetService<IFarmerSessionManager>(true);
    }

    public async Task Run()
    {
        var token = _managementTokenSource.Token;

        PrepareEnvironment();

        var result = await _sessionManager.LoginAsync(token);

        if (!result)
        {
            return;
        }

        _communicationHandler.NewLoggedUser += NewLoggedUser;

        await RunCore(token);

        _communicationHandler.NewLoggedUser -= NewLoggedUser;
    }

    private void NewLoggedUser(object sender, EventArgs args)
    {
        throw new NotSupportedException();
    }

    private async Task RunCore(CancellationToken token)
    {
        FillOperationalManagers();

        await _localInfoManager.InitializeGardensAsync(token);

        var tasks = new List<Task>();
        foreach (var opManager in _operationalManagers)
        {
            tasks.Add(Task.Run(() => opManager.Run(token)));
        }

        await Task.WhenAny(tasks);

        foreach (var opManager in _operationalManagers)
        {
            try
            {
                opManager.Dispose();
            }
            catch (Exception ex)
            {
                SmartFarmerLog.Exception(ex);
            }
        }
    }

    private void FillOperationalManagers()
    {
        if (_operationalManagers == null) _operationalManagers = new List<IOperationalModeManager>();

        _operationalManagers.ForEach(opMan => 
            {
                opMan.NewOperationRequired -= ExecuteRequiredOperation;
                opMan.Dispose();
            }
        );

        _operationalManagers.Clear();

        if (_configProvider.GetAppConfiguration().AppOperationalMode == null)
        {
            return;
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Console))
        {
            var console = new ConsoleOperationalModeManager();
            _operationalManagers.Add(console);

            FarmerServiceLocator.MapService<IConsoleOperationalModeManager>(() => console);
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Auto))
        {
            var auto = new AutomaticOperationalManager(_configProvider.GetAppConfiguration());
            _operationalManagers.Add(auto);

            FarmerServiceLocator.MapService<IAutoOperationalModeManager>(() => auto);
        }

        if (_configProvider.GetAppConfiguration().AppOperationalMode.Value.HasFlag(AppOperationalMode.Cli))
        {
            var cli = new CliOperationalManager(_configProvider.GetHubConfiguration());
            _operationalManagers.Add(cli);

            FarmerServiceLocator.MapService<ICliOperationalModeManager>(() => cli);
        }

        _operationalManagers.ForEach(async opMan =>
        {
            await opMan.InitializeAsync(_managementTokenSource.Token);
            opMan.NewOperationRequired += ExecuteRequiredOperation;
        });
    }

    private void ExecuteRequiredOperation(object sender, OperationRequestEventArgs e)
    {
        _requestExecutor.Execute(sender, e);
    }

    private void PrepareEnvironment()
    {
        // SmartFarmerLog.SetShowThreadInfo(true);

    }
    
}
