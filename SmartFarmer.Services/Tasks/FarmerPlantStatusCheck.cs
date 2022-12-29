﻿using System.Collections.Generic;
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
                new FarmerPlanStep(FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeafDetector))),
                new FarmerPlanStep(FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerLeavesStatusChecker))),
                new FarmerPlanStep(FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerStemDetector))),
                new FarmerPlanStep(FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerParasiteChecker))),
                new FarmerPlanStep(FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerHydrationLevelChecker)))
            };
        }

        #endregion
    }
}
