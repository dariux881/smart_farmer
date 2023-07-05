using System.Threading.Tasks;
using SmartFarmer.FarmerLogs;

namespace SmartFarmer.AI.Base;

public interface ISmartFarmerAIDetector<T> : ISmartFarmerAIModule
{
    Task<FarmerAIDetectionLog> ExecuteDetection(object stepData);
}
