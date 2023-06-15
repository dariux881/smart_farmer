using SmartFarmer.Alerts;

namespace SmartFarmer.Alerts;

public class FarmerAlertRequestData
{    
    public string RaisedByTaskId { get; set; }
    public string PlantInstanceId { get; set; }
    public string GardenId { get; set; }

    public AlertCode Code { get; set; }
    public string Message { get; set; }
    public AlertLevel Level { get; set; }
    public AlertSeverity Severity { get; set; }
}