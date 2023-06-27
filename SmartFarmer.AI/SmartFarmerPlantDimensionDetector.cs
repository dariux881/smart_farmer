using System.Threading.Tasks;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.AI;

public class SmartFarmerPlantDimensionDetector : ISmartFarmerAIGardenModule
{
    public FarmerAIDetectionLog Log { get; set; }

    public Task<IFarmerPlan> GenerateHoverPlan(IFarmerGarden garden)
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