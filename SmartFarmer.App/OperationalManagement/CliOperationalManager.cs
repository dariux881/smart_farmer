using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Configurations;
using SmartFarmer.Data.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Handlers;
using SmartFarmer.Misc;
using SmartFarmer.Position;
using SmartFarmer.Tasks;

namespace SmartFarmer.OperationalManagement;

public class CliOperationalManager : OperationalModeManagerBase, ICliOperationalModeManager
{
    private HubConnectionConfiguration _hubConfiguration;
    private Dictionary<string, FarmerGardenHubHandler> _hubHandlers;
    private readonly IFarmerAppCommunicationHandler _appCommunication;
    private readonly IFarmerLocalInformationManager _localInfoManager;
    private SemaphoreSlim _commandSem;
    private IFarmerCliCommand _localCommand;
    private CancellationToken _operationsToken;

    public CliOperationalManager(HubConnectionConfiguration hubConfiguration)
    {
        _hubConfiguration = hubConfiguration;
        _commandSem = new SemaphoreSlim(1);
        _hubHandlers = new Dictionary<string, FarmerGardenHubHandler>();

        _localInfoManager = FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true);
        _appCommunication = FarmerServiceLocator.GetService<IFarmerAppCommunicationHandler>(true);

        _appCommunication.LocalGardenAdded += LocalGardenAdded;
        _appCommunication.LocalGardenRemoved += LocalGardenRemoved;
    }

    public override AppOperationalMode Mode => AppOperationalMode.Cli;
    public override string Name => "Remote CLI";

    public override async Task InitializeAsync(CancellationToken token)
    {
        // Configuring hubs
        await Task.CompletedTask;
    }

    public override async Task Run(CancellationToken token)
    {
        _operationsToken = token;
        
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000);
            }

            await Task.CompletedTask;
        }
        catch(AggregateException ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }

        SmartFarmerLog.Information("closing Cli manager");
    }

    public override void ProcessResult(OperationRequestEventArgs args)
    {
        Task.Run(async () => await NotifyResult(args.Result));
    }

    public override void Dispose()
    {
        if (_appCommunication != null)
        {
            _appCommunication.LocalGardenAdded -= LocalGardenAdded;
            _appCommunication.LocalGardenRemoved -= LocalGardenRemoved;
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

    private void LocalGardenAdded(object sender, GardenChangedEventArgs e)
    {
        if (_hubHandlers.ContainsKey(e.GardenId))
        {
            return;
        }

        var hub = new FarmerGardenHubHandler(_localInfoManager.Gardens[e.GardenId], _hubConfiguration);
        ConfigureHub(hub);
        
        _hubHandlers.Add(e.GardenId, hub);
    }

    private void LocalGardenRemoved(object sender, GardenChangedEventArgs e)
    {
        if (!_hubHandlers.TryGetValue(e.GardenId, out var hub))
        {
            return;
        }

        hub.NewCliCommandReceived -= NewCliCommandReceived;
        _hubHandlers.Remove(e.GardenId);

        Task.Run(async () => await hub.DisposeAsync());
    }

    private void ConfigureHub(FarmerGardenHubHandler hub)
    {
        hub.NewCliCommandReceived += NewCliCommandReceived;
        Task.Run(async () => await hub.InitializeAsync(_operationsToken));
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

                case "stop":
                    isCommandValid = true;
                    SendNewOperation(AppOperation.StopCurrentOperation, null);
                    break;
            }

            if (!isCommandValid)
            {
                Task.Run(async () => 
                    {
                        await NotifyResult(
                            new FarmerPlanExecutionResult()
                            {
                                LastException = new Exception($"received command {command} is not valid")
                            },
                            false);
                            
                        ResetLocalCommand();
                    });
            }

        }
        catch(AggregateException ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
        }
        finally
        {
            _commandSem.Release();
        }
    }

    private void ResetLocalCommand()
    {
        _localCommand = null;
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
                    SendNewOperation(
                        AppOperation.RunPlan, 
                        new [] { planId });
                }

                break;
            
            case "-autoPlan":
                {
                    outcome = true;
                    SendNewOperation(
                        AppOperation.RunAutoIrrigationPlan, 
                        new [] { command.GardenId });                    
                }

                break;
        }

        return outcome;
    }

    private bool ProcessMoveCommand(IFarmerCliCommand command)
    {
        bool outcome = false;
        if (!command.Args.Any()) return outcome;

        var point = new Farmer5dPoint();

        foreach (var arg in command.Args)
        {
            switch (arg.Key)
            {
                case "-x":
                    point.X = arg.Value.First().GetDouble();
                    outcome = true;
                    break;
                case "-y":
                    point.Y = arg.Value.First().GetDouble();
                    outcome = true;
                    break;
                case "-z":
                    point.Z = arg.Value.First().GetDouble();
                    outcome = true;
                    break;
                case "-alpha":
                    point.Alpha = arg.Value.First().GetDouble();
                    outcome = true;
                    break;
                case "-beta":
                    point.Beta = arg.Value.First().GetDouble();
                    outcome = true;
                    break;
            }
        }

        if (outcome)
        {
            SendNewOperation(AppOperation.MoveToPosition, new [] { command.GardenId, point.Serialize() });
        }

        return outcome;
    }

    private async Task NotifyResult(IFarmerPlanExecutionResult result, bool notifyToServer = true)
    {
        if (_localCommand == null) return;

        var messageToSend = result.IsSuccess ? $"{result.PlanId} completed successfully" : result.ErrorMessage;
        
        if (notifyToServer)
        {
            await NotifyPlanExecutionResult(result);
        }

        await _hubHandlers[_localCommand.GardenId].NotifyCliCommandResult(_localCommand.GardenId, messageToSend, _operationsToken);
    }

    private void NewCliCommandReceived(object sender, NewCliCommandEventArgs e)
    {
        ProcessCliCommand(e.Command);
    }
}
