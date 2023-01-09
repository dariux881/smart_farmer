using System.Collections.Generic;
using SmartFarmer.Alerts;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;

namespace SmartFarmer
{
    public interface IFarmerGround
    {
        string ID { get; }
        string GroundName { get; }
        double Latitude { get; }
        double Longitude { get; }

        IReadOnlyList<IFarmerPlantInstance> Plants { get; }
        ICollection<IFarmerPlan> Plans { get; }
        ICollection<IFarmerAlert> Alerts { get; }
        IFarmerAutoIrrigationPlan GroundIrrigationPlan { get; }
        double WidthInMeters { get; }
        double LengthInMeters { get; }

        void AddPlants(IFarmerPlantInstance[] plants);
        void AddPlant(IFarmerPlantInstance plant);
        void RemovePlant(IFarmerPlantInstance plant);
    }
}
