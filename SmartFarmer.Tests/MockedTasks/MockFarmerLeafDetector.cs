using System;
using System.Threading;
using System.Threading.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Tasks.PlantUtils;
using SmartFarmer.Utils;

namespace SmartFarmer.MockedTasks
{
    public abstract class MockTaskBase : IFarmerTask
    {
        public bool ExpectFail { get; set; } 

        public FarmerTool RequiredTool => FarmerTool.None;
        
        public bool IsInProgress { get; protected set; }

        public Exception LastException { get; protected set; }

        public async Task Execute(object[] parameters, CancellationToken token)
        {
            if (ExpectFail)
            {
                throw new Exception();
            }

            await Task.CompletedTask;
        }
    }

    public class MockFarmerLeafDetector : MockTaskBase, IFarmerLeafDetector { }
    public class MockFarmerLeavesStatusChecker : MockTaskBase, IFarmerLeavesStatusChecker { }
    public class MockFarmerStemDetector : MockTaskBase, IFarmerStemDetector { }
}