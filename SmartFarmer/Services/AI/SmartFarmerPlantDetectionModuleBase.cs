using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.AI;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Plants;
using SmartFarmer.Tasks.Detection;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Services.AI;

public class SmartFarmerPlantDetectionModuleBase : ISmartFarmerAIPlantPlanGenerator
{
    public string PlantId => null;
    public string PlantBotanicalName => null;

    public async Task<IFarmerPlan> GenerateHoverPlan(IFarmerPlantInstance plant)
    {
        var plan = new FarmerPlan();

        plan.Name = "Hover plan for " + plant.ID;
        
        var centerX = plant.PlantX;
        var centerY = plant.PlantY;
        var xBound = Math.Max(plant.PlantWidth / 2, 10);
        var yBound = Math.Max(plant.PlantDepth / 2, 10);

        // go to target height
        plan.Steps.Add(
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasPlantInstanceReference.PlantInstanceID), plant.ID }
                    },
        });
        
        // go to centerX, centerY - yBound
        plan.Steps.Add(
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasPlantInstanceReference.PlantInstanceID), plant.ID }
                    },
        });
        
        // go to centerX + xBound / 2, centerY
        plan.Steps.Add(
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasPlantInstanceReference.PlantInstanceID), plant.ID }
                    },
        });

        // go to centerX, centerY + yBound
        plan.Steps.Add(
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
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
            new FarmerPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IHasPlantInstanceReference.PlantInstanceID), plant.ID }
                    },
        });

        await Task.CompletedTask;

        return plan;
    }
}
