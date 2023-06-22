using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerHoverPlanResult
{
    public string PlanId { get; set; }
    public string PlantInstanceId { get; set; }
    public Dictionary<string, string> ImagesFilenameByStep { get; set; }
}