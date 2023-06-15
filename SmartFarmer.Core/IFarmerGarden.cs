using System.Collections.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer;

public interface IFarmerGarden : IFarmerService
{
    string GardenName { get; }
    double Latitude { get; }
    double Longitude { get; }
    string UserID { get; }

    IReadOnlyList<string> PlantIds { get; }
    IReadOnlyList<string> PlanIds { get; }
    IReadOnlyList<string> AlertIds { get; }

    bool CanIrrigationPlanStart { get; }
    string IrrigationPlanId { get; }
    double WidthInMeters { get; }
    double LengthInMeters { get; }

    void AddPlants(string[] plantIds);
    void AddPlant(string plantId);
    void RemovePlant(string plantId);

    void AddPlan(string planId);
    void RemovePlan(string planId);

    void AddAlert(string alertId);
    void RemoveAlert(string alertId);
    void MarkAlertAsRead(string alertId, bool read);
}
