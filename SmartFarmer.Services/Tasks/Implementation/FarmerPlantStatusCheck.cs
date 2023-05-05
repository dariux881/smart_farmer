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
                new FarmerPlanStep(ID + "_1", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeafDetectorTask))),
                new FarmerPlanStep(ID + "_2", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeavesStatusCheckerTask))),
                new FarmerPlanStep(ID + "_3", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerStemDetectorTask))),
                new FarmerPlanStep(ID + "_4", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerParasiteCheckerTask))),
                new FarmerPlanStep(ID + "_5", FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerHydrationLevelCheckerTask)))
            };
        }

        #endregion
    }
}
