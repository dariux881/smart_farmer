using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Exceptions;
using SmartFarmer.Handlers;
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
    public int PumpNumber { get; set; }
    public double WaterAmountInLiters { get; set; }

    public void Dispose() 
    {

    }

    public override async Task Execute(CancellationToken token)
    {
        Exception _ex = null;
        PrepareTask();

        try
        {
            var waterNeeded = await _waterCheckerTask.IsWaterNeeded(WaterAmountInLiters, token);
            if (!waterNeeded)
            {
                SmartFarmerLog.Information("irrigation skipped since water is not needed");
                return;
            }

            await ProvideWater(PumpNumber, WaterAmountInLiters, token);
        }
        catch(Exception ex)
        {
            _ex = ex;
            throw;
        }
        finally
        {
            EndTask(_ex != null);
        }
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

