using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks;

namespace SmartFarmer.OperationalManagement;

public class CliOperationalManager : ICliOperationalModeManager
{
    private HubConnectionConfiguration _hubConfiguration;
    private Dictionary<string, FarmerGroundHubHandler> _hubHandlers;
    private readonly IFarmerAppCommunicationHandler _appCommunication;
    private SemaphoreSlim _commandSem;
    private IFarmerCliCommand _localCommand;

    public CliOperationalManager(HubConnectionConfiguration hubConfiguration)
    {
        _hubConfiguration = hubConfiguration;
        _commandSem = new SemaphoreSlim(1);

        _hubHandlers = new Dictionary<string, FarmerGroundHubHandler>();
        _appCommunication = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);

        _appCommunication.LocalGroundAdded += LocalGroundAdded;
        _appCommunication.LocalGroundRemoved += LocalGroundRemoved;
    }

    public AppOperationalMode Mode => AppOperationalMode.Cli;
    public string Name => "Remote CLI";
    public event EventHandler<OperationRequestEventArgs> NewOperationRequired;

    public async Task InitializeAsync()
    {
        // Configuring hubs
        await Task.CompletedTask;
    }

    public async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(5000);
        }

        await Task.CompletedTask;
    }

    public void ProcessResult(OperationRequestEventArgs args)
    {
        if (args.ExecutionException != null)
        {
            SmartFarmerLog.Exception(args.ExecutionException);
        }

        if (args.Result != null)
        {
            SmartFarmerLog.Debug(args.Result);
        }

        Task.Run(async () => await NotifyResult(args.ExecutionException?.ToString() ?? args.Result));
    }

    public void Dispose()
    {
        if (_appCommunication != null)
        {
            _appCommunication.LocalGroundAdded -= LocalGroundAdded;
            _appCommunication.LocalGroundRemoved -= LocalGroundRemoved;
        }

        if (_hubHandlers != null)
        {
            foreach (var hub in _hubHandlers.Values)
            {
                hub.NewCliCommandReceived -= NewCliCommandReceived;
                Task.Run(async () => await hub.DisposeAsync());
            }
        }
    }

    private void LocalGroundAdded(object sender, GroundChangedEventArgs e)
    {
        if (_hubHandlers.ContainsKey(e.GroundId))
        {
            return;
        }

        var hub = new FarmerGroundHubHandler(LocalConfiguration.Grounds[e.GroundId], _hubConfiguration);
        ConfigureHub(hub);
        
        _hubHandlers.Add(e.GroundId, hub);
    }

    private void LocalGroundRemoved(object sender, GroundChangedEventArgs e)
    {
        if (!_hubHandlers.TryGetValue(e.GroundId, out var hub))
        {
            return;
        }

        hub.NewCliCommandReceived -= NewCliCommandReceived;
        _hubHandlers.Remove(e.GroundId);

        Task.Run(async () => await hub.DisposeAsync());
    }

    private void ConfigureHub(FarmerGroundHubHandler hub)
    {
        hub.NewCliCommandReceived += NewCliCommandReceived;
        Task.Run(async () => await hub.InitializeAsync());
    }

    private void ProcessCliCommand(IFarmerCliCommand command)
    {
        _commandSem.Wait();

        try
        {
            _localCommand = command;
            SmartFarmerLog.Debug($"processing command {command}");

            //TODO invoke event
        }
        finally
        {
            _commandSem.Release();
        }
    }

    private async Task NotifyResult(string result)
    {
        if (_localCommand == null) return;

        await _hubHandlers[_localCommand.GroundId].NotifyCliCommandResult(_localCommand.GroundId, result);
    }

    private void NewCliCommandReceived(object sender, NewCliCommandEventArgs e)
    {
        ProcessCliCommand(e.Command);
    }
}
