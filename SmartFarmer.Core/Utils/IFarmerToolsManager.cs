using System.Threading;
using System.Threading.Tasks;

namespace SmartFarmer.Utils;

public interface IFarmerToolsManager
{
    FarmerTool GetCurrentlyMountedTool();
    Task MountTool(FarmerTool tool, CancellationToken token);
    void SetToolCollectorPosition(Farmer2dPoint toolsCollectorPosition);
}
