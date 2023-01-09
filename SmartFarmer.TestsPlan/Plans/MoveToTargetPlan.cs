using System.Collections.Generic;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Tests;

public class MoveToTargetPlan : BasePlan
{
    public MoveToTargetPlan()
        : base("move to target")
    {
        EditableSteps = new List<IFarmerPlanStep>()
            {
                // moving to origin
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new object[] {(int)0, (int)0}),
                // moving to target X
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new object[] {(int)10, (int)0}),
                // moving to target point
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new object[] {(int)10, (int)10}),
                // moving to target height
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveArmAtHeight)),
                    new object[] {(int)60}),
                // moving to target degree
                new FarmerPlanStep(
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerTurnArmToDegree)),
                    new object[] {(double)45.0}),
            };
    }
}