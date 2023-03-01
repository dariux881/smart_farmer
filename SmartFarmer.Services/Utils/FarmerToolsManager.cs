using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Utils
{
    public class FarmerToolsManager
    {
        private static readonly Lazy<FarmerToolsManager> _instance = new(() => new FarmerToolsManager());
        private IFarmerToolManager _toolManager;

        public static FarmerToolsManager Instance => _instance.Value;

        public FarmerToolsManager()
        {
            _toolManager = 
                FarmerDiscoveredTaskProvider
                    .GetTaskDelegateByInterfaceFullName(
                        typeof(IFarmerToolManager).FullName) as IFarmerToolManager;
        }

        public FarmerTool GetCurrentlyMountedTool()
        {
            if (_toolManager == null) throw new InvalidOperationException("invalid tool manager");
            return _toolManager.GetMountedTool();
        }

        public async Task MountTool(FarmerTool tool, CancellationToken token)
        {
            if (_toolManager == null) throw new InvalidOperationException("invalid tool manager");

            // refer to external tool replace executor
            await _toolManager.MountTool(tool, token);
        }
    }
}
