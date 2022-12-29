using SmartFarmer.Tasks;
using System.Collections.Generic;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Plants;

namespace SmartFarmer
{
    public interface IFarmerGround
    {
        IReadOnlyList<IFarmerPlantInstance> Plants { get; }
        ICollection<IFarmerPlan> Plans { get; }
        IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; }
        double WidthInMeters { get; }
        double LengthInMeters { get; }

        void AddPlants(IFarmerPlantInstance[] plants);
        void AddPlant(IFarmerPlantInstance plant);
        void RemovePlant(IFarmerPlantInstance plant);
    }
}
