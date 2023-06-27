using System;
using System.Threading.Tasks;
using SmartFarmer.Tasks;

namespace SmartFarmer.AI;

public class SmartFarmerTestModule : SmartFarmerPlantDetectionModuleBase
{
    public override string PlantId => null;
    public override string PlantBotanicalName => "PlantBotanicalName_1";

    public override async Task<FarmerAIDetectionLog> ExecuteDetection(FarmerHoverPlanExecutionResult planResult)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}