using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Helpers;
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

    public override async Task Execute(object[] parameters, CancellationToken token)
    {
        if (parameters == null || parameters.Length < 1) throw new ArgumentException(nameof(parameters));

        var expectedAmountInLiters = parameters[0].GetDouble();

        Exception _ex = null;
        PrepareTask();

        try
        {
            await IsWaterNeeded(expectedAmountInLiters, token);
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