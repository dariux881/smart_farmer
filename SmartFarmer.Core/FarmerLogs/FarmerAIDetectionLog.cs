using System.Collections.Generic;
using System.Linq;

namespace SmartFarmer.FarmerLogs;

public class FarmerAIDetectionLog
{
    public FarmerAIDetectionLog()
    {
        Messages = new List<FarmerAIDetectionLogMessage>();
    }

    public FarmerAIDetectionLog(FarmerAIDetectionLogMessage message)
        : this()
    {
        if (message != null) Messages.Add(message);
    }

    public FarmerAIDetectionLog(IList<FarmerAIDetectionLogMessage> messages)
        : this()
    {
        if (messages != null && messages.Any()) Messages.AddRange(messages);
    }

    public List<FarmerAIDetectionLogMessage> Messages { get; }

    public void InjectFrom(FarmerAIDetectionLog log)
    {
        if (log == null || log.Messages == null || !log.Messages.Any()) return;
        
        Messages.AddRange(log.Messages);
    }
}