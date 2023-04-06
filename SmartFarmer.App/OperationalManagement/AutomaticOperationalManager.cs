using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Helpers;

namespace SmartFarmer.OperationalManagement;

public class AutomaticOperationalManager : IOperationalModeManager
{
    public string Name => "Automatic Operational Manager";

    public event EventHandler<OperationRequestEventArgs> NewOperationRequired;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async Task Run()
    {
        var plans = GetPlansToRun();
        foreach (var plan in plans)
        {
            SendNewOperation(AppOperation.RunPlan, new[] { plan });
        }

        //TODO subscribe to Hub
        //TODO schedule plans for automatic activities
    }

    private static IEnumerable<string> GetPlansToRun()
    {
        var plans = new List<string>();

        foreach (var gGround in LocalConfiguration.Grounds.Values)
        {
            var ground = gGround as FarmerGround;
            if (ground == null) continue;
            
            var now = DateTime.UtcNow;
            var today = now.DayOfWeek;

            var plansInGround = 
                ground
                    .Plans
                        .Where(x => 
                            (x.ValidFromDt == null || x.ValidFromDt <= now) && // valid start
                            (x.ValidToDt == null || x.ValidToDt > now)) // valid end
                        .Where(x => x.PlannedDays == null || !x.PlannedDays.Any() || x.PlannedDays.Contains(today)) // valid day of the week
                        .OrderBy(x => x.Priority)
                        .Select(x => x.ID)
                        .ToList();
            
            plans.AddRange(plansInGround);
        }

        return plans;
    }

    protected void SendNewOperation(AppOperation operation, string[] data)
    {
        NewOperationRequired?.Invoke(this, new OperationRequestEventArgs(this, operation, data));
    }
}