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
        string UserID { get; }

        IReadOnlyList<string> PlantIds { get; }
        IReadOnlyList<string> PlanIds { get; }
        ICollection<IFarmerAlert> Alerts { get; }
        string GroundIrrigationPlanId { get; }
        double WidthInMeters { get; }
        double LengthInMeters { get; }

        void AddPlants(string[] plantIds);
        void AddPlant(string plantId);
        void RemovePlant(string plantId);

        void AddPlan(string planId);
        void RemovePlan(string planId);
    }
}
