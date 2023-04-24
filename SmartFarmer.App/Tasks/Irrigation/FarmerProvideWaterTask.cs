using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Exceptions;
using SmartFarmer.Helpers;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Irrigation;

public class FarmerProvideWaterTask : FarmerBaseTask, IFarmerProvideWaterTask, IDisposable
{
    private IFarmerWaterProviderDevice _handler;
    private IFarmerCheckIfWaterIsNeededTask _waterCheckerTask;

    public FarmerProvideWaterTask(IFarmerWaterProviderDevice handler)
    {
        this.RequiredTool = Utils.FarmerTool.Water;
        
        _handler = handler;
        _waterCheckerTask = new FarmerCheckIfWaterIsNeededTask(handler);
    }

    public override string TaskName => "Provides water to plant task";

    public void Dispose() 
    {

    }

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 2) throw new ArgumentException(nameof(parameters));

        var pumpNumber = parameters[0].GetInt();
        var amountOfWater = parameters[1].GetDouble();

        var waterNeeded = await _waterCheckerTask.IsWaterNeeded(amountOfWater, token);
        if (!waterNeeded)
        {
            SmartFarmerLog.Information("irrigation skipped since water is not needed");
            return;
        }

        await ProvideWater(pumpNumber, amountOfWater, token);
    }

    public async Task ProvideWater(int pumpNumber, double amountInLiters, CancellationToken token)
    {
        bool success = true;

        try
        {
            PrepareTask();

            var result = await _handler.ProvideWaterAsync(pumpNumber, amountInLiters, token);

            if (!result)
            {
                throw new FarmerTaskExecutionException(
                    this.ID, 
                    null,
                    "Water pump failed",
                    null,
                    Alerts.AlertCode.Unknown,
                    Alerts.AlertLevel.Warning,
                    Alerts.AlertSeverity.High);
            }

        }
        catch(FarmerTaskExecutionException)
        {
            success = false;
            throw;
        }
        catch(Exception ex)
        {
            SmartFarmerLog.Exception(ex);
            success = false;
        }
        finally {
            EndTask(!success);
        }
    }
}

