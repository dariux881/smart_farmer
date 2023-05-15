using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Misc;
using SmartFarmer.Handlers;

namespace SmartFarmer.OperationalManagement;

public class ConsoleOperationalModeManager : OperationalModeManagerBase, IConsoleOperationalModeManager
{
    private bool CanRun = true;
    private readonly IFarmerLocalInformationManager _localInfoManager;

    public ConsoleOperationalModeManager()
    {
        _localInfoManager = FarmerServiceLocator.GetService<IFarmerLocalInformationManager>(true);        
    }

    public override string Name => "Console Operational Manager";
    public override AppOperationalMode Mode => AppOperationalMode.Console;

    public override async Task InitializeAsync(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public override async Task Run(CancellationToken token)
    {
        var menu = PromptAndExecute(token);

        if (menu != -1)
        {
            SmartFarmerLog.Error($"Invalid choice {menu}");
        }

        Console.WriteLine("closing console mode");
        await Task.CompletedTask;
    }

    public override void ProcessResult(OperationRequestEventArgs args)
    {
        if (args.ExecutionException != null)
        {
            SmartFarmerLog.Error("received\n\t" + args.ExecutionException.Message);
        }

        if (args.Result != null)
        {
            SmartFarmerLog.Information(args.Result);
        }
    }

    public override void Dispose()
    {

    }

    private int PromptAndExecute(CancellationToken token) 
    {
        string message = 
            "\n"+
            "0 - list grounds\n" +
            "1 - list plans\n" +
            "2 - execute plan\n" +
            "3 - list alerts\n"+
            "4 - invert alert read flag\n"+
            "5 - update ground\n"+
            "6 - update grounds\n"+
            "7 - cli command\n" +
            "-1 - exit\n"+
            " select: ";

        int choice = -1;
        bool validAction;

        try 
        {
            do
            {
                if (!CanRun) break;
                if (token.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine(message);

                while (!int.TryParse(Console.ReadLine().Trim(), out choice))
                {
                    choice = -1;
                    Console.WriteLine("retry: \n select: ");
                }

                validAction = ExecuteAction(choice);
            }
            while (choice >= 0 && validAction);

            return choice;
        }
        catch (Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            return -1;
        }
    }

    private bool ExecuteAction(int choice)
    {

        switch (choice)
        {
            case -1:
                break;
            case 0: // list grounds
                {
                    Console.WriteLine("GROUNDS:");
                    foreach (var ground in _localInfoManager.Grounds.Select(x => x.Key))
                    {
                        Console.WriteLine("\t" + ground);
                    }
                }

                break;
            case 1: // list plans
                {
                    Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");

                    var groundId = GetGroundIdFromInputOrDefault();
                    var ground = _localInfoManager.Grounds[groundId] as FarmerGround;

                    Console.WriteLine($"PLANS OF GROUND {ground.GroundName}:");
                    foreach (var plan in ground.Plans)
                    {
                        Console.WriteLine($"\t{plan.ID} ({plan.StepIds?.Count} steps)");
                    }
                }

                break;
            case 2: // run plan
                {
                    Console.WriteLine("insert plan ID: ");
                    var planId = Console.ReadLine().Trim();
                    SendNewOperation(AppOperation.RunPlan, new [] { planId });
                }

                break;            
            case 3: // list alerts
                {
                    Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");

                    var groundId = GetGroundIdFromInputOrDefault();
                    if (!_localInfoManager.Grounds.TryGetValue(groundId, out var ground))
                    {
                        break;
                    }

                    var fGround = ground as FarmerGround;
                    if (fGround == null)
                    {
                        break;
                    }

                    Console.WriteLine($"ALERTS OF GROUND {ground.GroundName}:");
                    foreach (var alert in fGround.Alerts)
                    {
                        var readStatus = (alert.MarkedAsRead ? "" : "not ") + "read"; 
                        Console.WriteLine($"\t{alert.ID} - {alert.Code} ({alert.Level}/{alert.Severity}) - {readStatus}\n\t\t{alert.Message}");
                    }
                }

                break;
            case 4: // mark alert
                {
                    Console.WriteLine("insert alert ID: ");
                    var alertId = Console.ReadLine().Trim();
                    SendNewOperation(AppOperation.MarkAlert, new [] { alertId });
                }

                break;
            case 5: // update ground
                {
                    Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");
                    var groundId = GetGroundIdFromInputOrDefault();
                    SendNewOperation(AppOperation.UpdateGround, new [] { groundId });
                }

                break;
            case 6: // update grounds
                {
                    SendNewOperation(AppOperation.UpdateAllGrounds, null);
                }

                break;
            case 7: // cli
                {
                    HandleCli();
                }

                break;
            case 8: // test pos
                {
                    Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");
                    var groundId = GetGroundIdFromInputOrDefault();
                    SendNewOperation(AppOperation.TestPosition, new [] { groundId });
                }

                break;
            default:
                return false;
        }

        return true;
    }

    private void HandleCli()
    {
        PromptCli();
        ReceiveCliCommand(out var groundId, out var command);

        if (command == "quit")
        {
            return;
        }

        SendNewOperation(AppOperation.CliCommand, new [] { groundId, command });
    }

    private void PromptCli()
    {
        Console.WriteLine("[quit] to exit");
        Console.WriteLine("> ");
    }

    private void ReceiveCliCommand(out string groundId, out string command)
    {
        command = Console.ReadLine().Trim();

        if (command == "quit")
        {
            groundId = null;
            return;
        }

        Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");
        groundId = GetGroundIdFromInputOrDefault();
    }

    private string GetGroundIdFromInputOrDefault()
    {
        var input = Console.ReadLine().Trim();
        return
            string.IsNullOrEmpty(input) ? 
                GetDefaultGroundId() : 
                input;
    }

    private string GetDefaultGroundId() 
    {
        return _localInfoManager.Grounds.FirstOrDefault().Key;
    }
}
