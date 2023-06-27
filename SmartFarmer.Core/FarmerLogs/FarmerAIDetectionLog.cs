using System.Collections.Generic;

namespace SmartFarmer.FarmerLogs;

public class FarmerAIDetectionLog
{
    public FarmerAIDetectionLog()
    {
        Messages = new List<FarmerAIDetectionLogMessage>();
    }

    public List<FarmerAIDetectionLogMessage> Messages { get; }

    public void InjectFrom(FarmerAIDetectionLog log)
    {
        Messages.AddRange(log.Messages);
    }
}