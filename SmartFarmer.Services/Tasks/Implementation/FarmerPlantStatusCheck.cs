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
                new FarmerPlanStep(ID + "_1", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeafDetector))),
                new FarmerPlanStep(ID + "_2", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeavesStatusChecker))),
                new FarmerPlanStep(ID + "_3", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerStemDetector))),
                new FarmerPlanStep(ID + "_4", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerParasiteChecker))),
                new FarmerPlanStep(ID + "_5", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerHydrationLevelChecker)))
            };
        }

        #endregion
    }
}
