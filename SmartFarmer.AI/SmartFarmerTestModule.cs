using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartFarmer.Data;
using SmartFarmer.Plants;
using SmartFarmer.Tasks;
using SmartFarmer.Tasks.Detection;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.AI;

public class SmartFarmerTestModule : ISmartFarmerAIPlantModule
{
    public string PlantId => null;

    public string PlantBotanicalName => "PlantBotanicalName_1";

    public Task<bool> StartDetection(FarmerHoverPlanResult planResult)
    {
        throw new NotImplementedException();
    }

    public async Task<IFarmerHoverPlan> GenerateHoverPlan(IFarmerPlantInstance plant)
    {
        var plan = new FarmerHoverPlan();

        plan.ID = PlantBotanicalName + "_" + plant.ID + "_" + DateTime.UtcNow.ToString("G");
        plan.Name = "Hover plan for " + plant.ID;
        
        // go to plant.PlantX - plant.PlantWidth / 2, plant.PlantY
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IFarmerMoveOnGridTask.TargetXInCm), ""+(plant.PlantX - plant.PlantWidth) },
                        { nameof(IFarmerMoveOnGridTask.TargetYInCm), ""+plant.PlantY }
                    },
        });

        //TODO point to plant

        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });
        
        // go to plant.PlantX, plant.PlantY - plant.PlantDepth / 2
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IFarmerMoveOnGridTask.TargetXInCm), ""+(plant.PlantX) },
                        { nameof(IFarmerMoveOnGridTask.TargetYInCm), ""+(plant.PlantY - plant.PlantDepth / 2) }
                    },
        });

        //TODO point to plant
        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });
        
        // go to plant.PlantX + plant.PlantWidth / 2, plant.PlantY
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IFarmerMoveOnGridTask.TargetXInCm), ""+(plant.PlantX + plant.PlantWidth / 2) },
                        { nameof(IFarmerMoveOnGridTask.TargetYInCm), ""+(plant.PlantY) }
                    },
        });

        //TODO point to plant
        // take picture
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
        });

        //TODO go to plant.PlantX, plant.PlantY + plant.Depth / 2
        plan.Steps.Add(
            new FarmerHoverPlanStep() { 
                ID = plan.ID + "_" + plan.Steps.Count,
                TaskInterfaceFullName = typeof(IFarmerMoveOnGridTask).FullName,
                BuildParameters =
                    new Dictionary<string, string>() 
                    {
                        { nameof(IFarmerMoveOnGridTask.TargetXInCm), ""+(plant.PlantX) },
                        { nameof(IFarmerMoveOnGridTask.TargetYInCm), ""+(plant.PlantY + plant.PlantDepth / 2) }
                    },
        });

        //TODO point to plant

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