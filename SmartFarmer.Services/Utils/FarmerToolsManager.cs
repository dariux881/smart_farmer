using System;
using System.Threading.Tasks;

namespace SmartFarmer.Utils
{
    public class FarmerToolsManager
    {
        private static readonly Lazy<FarmerToolsManager> _instance = new(() => new FarmerToolsManager());

        public static FarmerToolsManager Instance => _instance.Value;

        public FarmerToolsManager()
        {
            _currentlyMountedTool = FarmerTool.None;
        }

        private FarmerTool _currentlyMountedTool;

        public FarmerTool GetCurrentlyMountedTool()
        {
            return _currentlyMountedTool;
        }

        public async Task MountTool(FarmerTool tool)
        {
            //TODO refer to external tool replace executor
            _currentlyMountedTool = tool;

            await Task.CompletedTask;
        }
    }
}
