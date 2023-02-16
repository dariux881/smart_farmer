using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;

namespace SmartFarmer.Tasks.Irrigation;

public interface IFarmerCheckIfWaterIsNeeded : IFarmerTask
{
    Task<bool> IsWaterNeeded(string plantInstanceId);
}