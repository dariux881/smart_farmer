using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public class SmartFarmerPlantDimensionDetector : ISmartFarmerAIGardenPlanGenerator
{
    public FarmerAIDetectionLog Log { get; set; }

    public Task<IFarmerPlan> GenerateHoverPlan(IFarmerGarden garden, string gardenId)
    {
        //TODO point to the floor
        //TODO foreach plant
            //TODO move to the plant
            //TODO take picture
            //TODO detect plant size
            //TODO store log

        throw new System.NotImplementedException();
    }

    public async Task<FarmerAIDetectionLog> ExecuteDetection(object stepData)
    {
        await Task.CompletedTask;
        throw new System.NotImplementedException();
    }
}