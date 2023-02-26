
using System;
using SmartFarmer.Alerts;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.DTOs.Alerts;

public class FarmerAlert : IFarmerAlert
{
    public DateTime When { get; set; }
    public FarmerPlanStep RaisedByTask { get; set; }
    public string RaisedByTaskId { get; set; }

    public FarmerPlantInstance PlantInstance { get; set; }
    public string PlantInstanceId { get; set; }

    public FarmerGround FarmerGround { get; set; }
    public string FarmerGroundId { get; set; }

    public string Code { get; set; }
    public string Message { get; set; }
    public AlertLevel Level { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool MarkedAsRead { get; set; }
    public string ID { get; set; }
}