using System.Collections.Generic;

namespace SmartFarmer.Tasks;

public class FarmerHoverPlanExecutionResult : FarmerPlanExecutionResult
{
    public string PlantInstanceId { get; set; }
    public Dictionary<string, string> ImagesFilenameByStep { get; set; }
}