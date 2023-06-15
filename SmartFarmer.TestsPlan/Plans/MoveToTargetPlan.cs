using System.Collections.Generic;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;
using SmartFarmer.Utils;

namespace SmartFarmer.Tests;

public class MoveToTargetPlan : BasePlan
{
    public MoveToTargetPlan()
        : base("id", "move to target")
    {
        EditableSteps = new List<IFarmerPlanStep>()
            {
                // moving to origin
                new FarmerPlanStep(
                    ID + "_1",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new Dictionary<string, string>()
                    {
                        { "TargetXInCm", ""+0 },
                        { "TargetYInCm", ""+0 }
                    }),
                // moving to target X
                new FarmerPlanStep(
                    ID + "_2",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new Dictionary<string, string>()
                    {
                        { "TargetXInCm", ""+10 },
                        { "TargetYInCm", ""+0 }
                    }),
                // moving to target point
                new FarmerPlanStep(
                    ID + "_3",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveOnGridTask)),
                    new Dictionary<string, string>()
                    {
                        { "TargetXInCm", ""+10 },
                        { "TargetYInCm", ""+10 }
                    }),
                // moving to target height
                new FarmerPlanStep(
                    ID + "_4",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerMoveArmAtHeightTask)),
                    new Dictionary<string, string>()
                    {
                        { "TargetHeightInCm", ""+60 }
                    }),
                // moving to target degree
                new FarmerPlanStep(
                    ID + "_5",
                    FarmerTaskProvider.GetTaskDelegateByType(typeof(IFarmerTurnArmToDegreeTask)),
                    new Dictionary<string, string>()
                    {
                        { "TargetDegrees", ""+45.0 }
                    }),
            };
    }
}