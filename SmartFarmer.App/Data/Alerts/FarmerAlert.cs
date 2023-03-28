
using System;
using SmartFarmer.Alerts;
using SmartFarmer.Data.Tasks;
using SmartFarmer.Data.Plants;

namespace SmartFarmer.Data.Alerts;

public class FarmerAlert : IFarmerAlert
{
    public DateTime When { get; set; }
    public FarmerPlanStep RaisedByTask { get; set; }
    public string RaisedByTaskId { get; set; }

    public FarmerPlantInstance PlantInstance { get; set; }
    public string PlantInstanceId { get; set; }

    public FarmerGround FarmerGround { get; set; }
    public string FarmerGroundId { get; set; }

    public AlertCode Code { get; set; }
    public string Message { get; set; }
    public AlertLevel Level { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool MarkedAsRead { get; set; }
    public string ID { get; set; }
}