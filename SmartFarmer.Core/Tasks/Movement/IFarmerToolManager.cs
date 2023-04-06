using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Movement;

public interface IFarmerToolManager : IFarmerTask
{
    Task MountTool(FarmerTool tool, CancellationToken token);
    FarmerTool GetMountedTool();
}