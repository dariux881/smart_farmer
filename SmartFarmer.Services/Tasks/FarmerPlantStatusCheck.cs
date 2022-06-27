using System.Collections.Generic;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Tasks.PlantUtils;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks
{
    public class FarmerPlantStatusCheckPlan : FarmerPlan
    {
        #region Constructors

        public FarmerPlantStatusCheckPlan()
        {
            Steps = new List<IFarmerPlanStep>()
            {
                new FarmerPlanStep() { Job = FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeafDetector)) },
                new FarmerPlanStep() { Job = FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeavesStatusChecker)) },
                new FarmerPlanStep() { Job = FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerStemDetector)) },
                new FarmerPlanStep() { Job = FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerParasiteChecker)) },
                new FarmerPlanStep() { Job = FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerHydrationLevelChecker)) }
            };
        }

        #endregion
    }
}
