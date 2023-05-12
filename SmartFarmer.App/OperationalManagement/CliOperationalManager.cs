using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Configurations;
using SmartFarmer.Handlers;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Tasks;

namespace SmartFarmer.OperationalManagement;

public class CliOperationalManager : OperationalModeManagerBase, ICliOperationalModeManager
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

    public override AppOperationalMode Mode => AppOperationalMode.Cli;
    public override string Name => "Remote CLI";

    public override async Task InitializeAsync()
    {
        // Configuring hubs
        await Task.CompletedTask;
    }

    public override async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(5000);
        }

        await Task.CompletedTask;
    }

    public override void ProcessResult(OperationRequestEventArgs args)
    {
        if (args.ExecutionException != null)
        {
            SmartFarmerLog.Error(args.ExecutionException.Message);
        }

        if (args.Result != null)
        {
            SmartFarmerLog.Debug(args.Result);
        }

        Task.Run(async () => await NotifyResult(args.ExecutionException?.ToString() ?? args.Result));
    }

    public override void Dispose()
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

            bool isCommandValid = false;
            switch(command.Command)
            {
                case "run":
                    isCommandValid = ProcessRunCommand(command);
                    break;

                case "move":
                    isCommandValid = ProcessMoveCommand(command);
                    break;
            }

            if (!isCommandValid)
            {
                Task.Run(async () => await NotifyResult($"received command {command} is not valid"));
            }

        }
        finally
        {
            _commandSem.Release();
        }
    }

    private bool ProcessRunCommand(IFarmerCliCommand command)
    {
        bool outcome = false;
        if (!command.Args.Any()) return outcome;

        var obj = command.Args.First();
        switch (obj.Key)
        {
            case "-plan":
                {
                    var planId = obj.Value.FirstOrDefault();
                    if (string.IsNullOrEmpty(planId)) break;

                    outcome = true;
                    SendNewOperation(AppOperation.RunPlan, new [] { planId });
                }

                break;
        }

        return outcome;
    }

    private bool ProcessMoveCommand(IFarmerCliCommand command)
    {
        return false;
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
