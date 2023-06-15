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
            "0 - list gardens\n" +
            "1 - list plans\n" +
            "2 - execute plan\n" +
            "3 - list alerts\n"+
            "4 - invert alert read flag\n"+
            "5 - update garden\n"+
            "6 - update gardens\n"+
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
            case 0: // list gardens
                {
                    Console.WriteLine("GARDENS:");
                    foreach (var garden in _localInfoManager.Gardens.Select(x => x.Key))
                    {
                        Console.WriteLine("\t" + garden);
                    }
                }

                break;
            case 1: // list plans
                {
                    Console.WriteLine("insert garden ID [" +  GetDefaultGardenId() +"]: ");

                    var gardenId = GetGardenIdFromInputOrDefault();
                    var garden = _localInfoManager.Gardens[gardenId] as FarmerGarden;

                    Console.WriteLine($"PLANS OF GARDEN {garden.GardenName}:");
                    foreach (var plan in garden.Plans)
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
                    Console.WriteLine("insert garden ID [" +  GetDefaultGardenId() +"]: ");

                    var gardenId = GetGardenIdFromInputOrDefault();
                    if (!_localInfoManager.Gardens.TryGetValue(gardenId, out var garden))
                    {
                        break;
                    }

                    var fGarden = garden as FarmerGarden;
                    if (fGarden == null)
                    {
                        break;
                    }

                    Console.WriteLine($"ALERTS OF GARDEN {garden.GardenName}:");
                    foreach (var alert in fGarden.Alerts)
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
            case 5: // update garden
                {
                    Console.WriteLine("insert garden ID [" +  GetDefaultGardenId() +"]: ");
                    var gardenId = GetGardenIdFromInputOrDefault();
                    SendNewOperation(AppOperation.UpdateGarden, new [] { gardenId });
                }

                break;
            case 6: // update gardens
                {
                    SendNewOperation(AppOperation.UpdateAllGardens, null);
                }

                break;
            case 7: // cli
                {
                    HandleCli();
                }

                break;
            case 8: // test pos
                {
                    Console.WriteLine("insert garden ID [" +  GetDefaultGardenId() +"]: ");
                    var gardenId = GetGardenIdFromInputOrDefault();
                    SendNewOperation(AppOperation.TestPosition, new [] { gardenId });
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
        ReceiveCliCommand(out var gardenId, out var command);

        if (command == "quit")
        {
            return;
        }

        SendNewOperation(AppOperation.CliCommand, new [] { gardenId, command });
    }

    private void PromptCli()
    {
        Console.WriteLine("[quit] to exit");
        Console.WriteLine("> ");
    }

    private void ReceiveCliCommand(out string gardenId, out string command)
    {
        command = Console.ReadLine().Trim();

        if (command == "quit")
        {
            gardenId = null;
            return;
        }

        Console.WriteLine("insert garden ID [" +  GetDefaultGardenId() +"]: ");
        gardenId = GetGardenIdFromInputOrDefault();
    }

    private string GetGardenIdFromInputOrDefault()
    {
        var input = Console.ReadLine().Trim();
        return
            string.IsNullOrEmpty(input) ? 
                GetDefaultGardenId() : 
                input;
    }

    private string GetDefaultGardenId() 
    {
        return _localInfoManager.Gardens.FirstOrDefault().Key;
    }
}
