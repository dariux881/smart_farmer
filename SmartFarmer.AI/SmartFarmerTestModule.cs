using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.AI;

public class SmartFarmerTestModule : SmartFarmerPlantDetectionModuleBase
{
    public override string PlantId => null;
    public override string PlantBotanicalName => "PlantBotanicalName_1";

    public override async Task<FarmerAIDetectionLog> ExecuteDetection(object stepData)
    {
        FarmerAIDetectionLog log = new FarmerAIDetectionLog();

        await Task.CompletedTask;
        SmartFarmerLog.Information($"processing {stepData}");

        return log;
    }
}