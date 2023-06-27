using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Misc;
using SmartFarmer.Movement;
using SmartFarmer.Tasks.Base;

namespace SmartFarmer.Tasks.Irrigation;

public class FarmerCheckIfWaterIsNeededTask : FarmerBaseTask, IFarmerCheckIfWaterIsNeededTask
{
    private IFarmerWaterProviderDevice _handler;

    public FarmerCheckIfWaterIsNeededTask(IFarmerWaterProviderDevice handler)
    {
        this.RequiredTool = Utils.FarmerTool.WaterSensor;
        
        _handler = handler;
    }

    public override string TaskName => "Check if water is needed for plant task";
    public double ExpectedAmountInLiters { get; set; }

    public override async Task<object> Execute(CancellationToken token)
    {
        Exception _ex = null;
        PrepareTask();

        try
        {
            return await IsWaterNeeded(ExpectedAmountInLiters, token);
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

    public async Task<bool> IsWaterNeeded(double expectedAmountInLiters, CancellationToken token)
    {
        return await _handler.GetCurrentHumidityLevel(token) <= GetExpectedHumidityByWater(expectedAmountInLiters);
    }

    private double GetExpectedHumidityByWater(double expectedAmountInLiters)
    {
        return expectedAmountInLiters * .5;
    }
}