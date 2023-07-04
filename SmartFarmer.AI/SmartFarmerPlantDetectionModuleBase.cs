using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.FarmerLogs;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Detection;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.AI;

public abstract class SmartFarmerPlantDetectionModuleBase : ISmartFarmerAIPlantModule
{
    public virtual string PlantId => null;
    public virtual string PlantBotanicalName => "PlantBotanicalName_1";

    public virtual async Task<FarmerAIDetectionLog> ExecuteDetection(object stepData)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
    
    public async Task<IFarmerPlan> GenerateHoverPlan(IFarmerPlantInstance plant)
    {
        var plan = new FarmerHoverPlan();

        plan.ID = PlantBotanicalName + "_" + plant.ID + "_" + DateTime.UtcNow.ToString("G");
        plan.Name = "Hover plan for " + plant.ID;
        
        var centerX = plant.PlantX;
        var centerY = plant.PlantY;
        var xBound = Math.Max(plant.PlantWidth / 2, 10);
        var yBound = Math.Max(plant.PlantDepth / 2, 10);

        // go to target height
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveArmAtHeightTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetHeight.TargetHeightInCm), "80" } //TODO fix: relate to max height
                    },
        });

        // target to plant
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTurnArmToDegreeTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetDegrees.TargetDegrees), "45" } //TODO fix: relate to max height
                    },
        });
        
        // go to centerX - xBound, centerY
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetGridPosition.TargetXInCm), ""+(centerX - xBound) },
                        { nameof(IHasTargetGridPosition.TargetYInCm), ""+centerY }
                    },
        });

        // point to plant
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTurnArmToDegreeTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetDegrees.TargetDegrees), "0" }
                    },
        });

        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });
        
        // go to centerX, centerY - yBound
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetGridPosition.TargetXInCm), ""+(centerX) },
                        { nameof(IHasTargetGridPosition.TargetYInCm), ""+(centerY - yBound) }
                    },
        });

        // point to plant
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTurnArmToDegreeTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetDegrees.TargetDegrees), "90" }
                    },
        });

        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });
        
        // go to centerX + xBound / 2, centerY
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetGridPosition.TargetXInCm), ""+(centerX + xBound) },
                        { nameof(IHasTargetGridPosition.TargetYInCm), ""+(centerY) }
                    },
        });

        // point to plant
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTurnArmToDegreeTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetDegrees.TargetDegrees), "180" }
                    },
        });
        
        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });

        // go to centerX, centerY + yBound
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetGridPosition.TargetXInCm), ""+(centerX) },
                        { nameof(IHasTargetGridPosition.TargetYInCm), ""+(centerY + yBound) }
                    },
        });

        // point to plant
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTurnArmToDegreeTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasTargetDegrees.TargetDegrees), "270" }
                    },
        });
        
        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });

        await Task.CompletedTask;

        return plan;
    }
}
