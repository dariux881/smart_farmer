using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.AI;

public class SmartFarmerTestModule : ISmartFarmerAIPlantDetector
{
    public string PlantId => null;
    public string PlantBotanicalName => "PlantBotanicalName_1";

    public async Task<FarmerAIDetectionLog> ExecuteDetection(object stepData)
    {
        FarmerAIDetectionLog log = new FarmerAIDetectionLog();

        await Task.CompletedTask;
        SmartFarmerLog.Information($"processing {stepData}");

        return log;
    }
}