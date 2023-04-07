using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;

namespace SmartFarmer.OperationalManagement;

public class ConsoleOperationalModeManager : IOperationalModeManager
{
    private bool CanRun = true;
    public event EventHandler<OperationRequestEventArgs> NewOperationRequired;
    public string Name => "Console Operational Manager";

    public async Task Prepare()
    {
        await Task.CompletedTask;
    }

    public async Task Run(CancellationToken token)
    {
        var menu = PromptAndExecute(token);

        if (menu != -1)
        {
            SmartFarmerLog.Error($"Invalid choice {menu}");
        }

        Console.WriteLine("closing console mode");
        await Task.CompletedTask;
    }

    public void Dispose()
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
            "5 - update grounds\n"+
            "-1 - exit\n"+
            " select: ";

        int choice = -1;
        bool validAction;

        do
        {
            if (!CanRun) break;
            if (token.IsCancellationRequested) break;

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

    private bool ExecuteAction(int choice)
    {

        switch (choice)
        {
            case -1:
                break;
            case 0: // list grounds
                {
                    Console.WriteLine("GROUNDS:");
                    foreach (var ground in LocalConfiguration.Grounds.Select(x => x.Key))
                    {
                        Console.WriteLine("\t" + ground);
                    }
                }

                break;
            case 1: // list plans
                {
                    Console.WriteLine("insert ground ID [" +  GetDefaultGroundId() +"]: ");

                    var groundId = GetGroundIdFromInputOrDefault();
                    var ground = LocalConfiguration.Grounds[groundId] as FarmerGround;

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
                    if (!LocalConfiguration.Grounds.TryGetValue(groundId, out var ground))
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
                        Console.WriteLine($"\t{alert.ID} - ({alert.Level}/{alert.Severity}) - read {alert.MarkedAsRead}\n\t\t{alert.Message}");
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
            default:
                return false;
        }

        return true;
    }

    protected void SendNewOperation(AppOperation operation, string[] data)
    {
        NewOperationRequired?.Invoke(this, new OperationRequestEventArgs(this, operation, data));
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
        return LocalConfiguration.Grounds.FirstOrDefault().Key;
    }
}
