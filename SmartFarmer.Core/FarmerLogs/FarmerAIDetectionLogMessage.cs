namespace SmartFarmer.FarmerLogs;

public class FarmerAIDetectionLogMessage
{
    public string Message { get; set; }
    public string StepID { get; set; }
    
    //TODO evaluate
    public bool RequiresAction { get; set; }
    public LogMessageLevel Level { get; set; }
}

public enum LogMessageLevel
{
    Error,
    Warning,
    Information
}