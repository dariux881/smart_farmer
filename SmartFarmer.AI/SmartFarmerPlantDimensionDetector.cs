using System.Threading.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;

namespace SmartFarmer.AI;

public class SmartFarmerPlantDimensionDetector : ISmartFarmerAIGardenModule
{
    public FarmerAIDetectionLog Log { get; set; }

    public Task<IFarmerHoverPlan> GenerateHoverPlan(IFarmerGarden garden)
    {
        //TODO point to the floor
        //TODO foreach plant
            //TODO move to the plant
            //TODO take picture
            //TODO detect plant size
            //TODO store log

        throw new System.NotImplementedException();
    }

    public async Task<FarmerAIDetectionLog> ExecuteDetection(FarmerHoverPlanResult planResult)
    {
        await Task.CompletedTask;
        throw new System.NotImplementedException();
    }
}