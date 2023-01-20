using System.Collections.Generic;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Health;
using SmartFarmer.Tasks.PlantUtils;
using SmartFarmer.Utils;

namespace SmartFarmer.Tasks.Implementation
{
    public class FarmerPlantStatusCheckPlan : FarmerPlan
    {
        #region Constructors

        public FarmerPlantStatusCheckPlan(string id)
            : base(id, "Plant Status Check")
        {
            EditableSteps = new List<IFarmerPlanStep>()
            {
                new FarmerPlanStep(ID + "_1", FarmerDiscoveredTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeafDetector))),
                new FarmerPlanStep(ID + "_2", FarmerDiscoveredTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeavesStatusChecker))),
                new FarmerPlanStep(ID + "_3", FarmerDiscoveredTaskProvider.GetTaskDelegateByType(typeof(IFarmerStemDetector))),
                new FarmerPlanStep(ID + "_4", FarmerDiscoveredTaskProvider.GetTaskDelegateByType(typeof(IFarmerParasiteChecker))),
                new FarmerPlanStep(ID + "_5", FarmerDiscoveredTaskProvider.GetTaskDelegateByType(typeof(IFarmerHydrationLevelChecker)))
            };
        }

        #endregion
    }
}
