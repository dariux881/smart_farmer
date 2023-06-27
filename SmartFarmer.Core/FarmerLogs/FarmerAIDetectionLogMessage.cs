namespace SmartFarmer.FarmerLogs;

public class FarmerAIDetectionLogMessage
{
    public string Message { get; set; }
    public string StepID { get; set; }
    
    //TODO evaluate
    public bool RequiresAction { get; set; }
}
