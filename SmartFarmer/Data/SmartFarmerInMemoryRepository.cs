using System.Collections.Generic;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Helpers;
using SmartFarmer.Tasks.Detection;
using SmartFarmer.Tasks.Generic;
using SmartFarmer.Tasks.Irrigation;
using SmartFarmer.Tasks.Movement;

namespace SmartFarmer.Data;

public class SmartFarmerInMemoryRepository : SmartFarmerRepository
{
    private static bool populated = false;

    public SmartFarmerInMemoryRepository(SmartFarmerDbContext dbContext)
        : base(dbContext)
    {
        PopulateDB();
    }

    private void PopulateDB()
    {
        if (populated) return;
        populated = true;

        var user = new User { ID = "user0", UserName="test", Password="test", Email="prova@test.it"};

        var readGardenAuth = new Authorization { ID = Constants.AUTH_READ_GARDEN, Name="Read Garden" };
        var editGardenAuth = new Authorization { ID = Constants.AUTH_EDIT_GARDEN, Name="Change Garden adding plans and plants" };
        var readUsersAuth = new Authorization { ID = Constants.AUTH_READ_USERS, Name="See current users" };
        var editUsersAuth = new Authorization { ID = Constants.AUTH_EDIT_USERS, Name="Edit users, creating or deleting them, changing their permissions" };

        user.Authorizations.Add(readGardenAuth);
        user.Authorizations.Add(editGardenAuth);

        var irrInfo1 = new FarmerIrrigationTaskInfo {
            //ID = "ii0",
            AmountOfWaterInLitersPerTime = 1,
            TimesPerWeek = 1
        };
        var irrInfo2 = new FarmerIrrigationTaskInfo {
            //ID = "ii1",
            AmountOfWaterInLitersPerTime = 5,
            TimesPerWeek = 1
        };

        _dbContext.SaveChanges();

        var plant1 = new FarmerPlant {
            ID = "plant1",
            BotanicalName = "PlantBotanicalName_1",
            FriendlyName="plant 1", 
            PlantWidth = 40,
            PlantDepth = 40,
            IrrigationTaskInfo = irrInfo1 };
        var plant2 = new FarmerPlant { 
            ID = "plant2",
            BotanicalName = "PlantBotanicalName_2",
            FriendlyName="plant 2", 
            PlantWidth = 40,
            PlantDepth = 40,
            IrrigationTaskInfo = irrInfo2 };

        var garden1 = new DTOs.FarmerGarden(this)
        { 
            ID = "gID", 
            UserID="user0", 
            GardenName="Garden Name",
        };

        _dbContext.Gardens.Add(garden1);
        _dbContext.Plants.AddRange(new [] {plant1, plant2});
        _dbContext.SaveChanges();

        var plantInstance1 = new FarmerPlantInstance { 
            ID = "PI1", 
            PlantKindID=plant1.ID, 
            PlantName=plant1.FriendlyName, 
            PlantX=1, 
            PlantY=2,
            FarmerGardenId = garden1.ID 
        };

        var plantInstance2 = new FarmerPlantInstance { 
            ID = "PI2",
            PlantKindID=plant2.ID, 
            PlantName="pianta", 
            PlantX=10, 
            PlantY=22,
            FarmerGardenId = garden1.ID 
        };

        var plan0 = new FarmerPlan
        {
            GardenId = garden1.ID,
            ID = "defaultPlan",
            Name = "Test plan"
        };

        // var plan1 = new FarmerPlan
        // {
        //     GardenId = garden1.ID,
        //     //ID = "plan1",
        //     Name = "Monday plan",
        //     CronSchedule = "* * * ? * MON *"
        // };

        // var plan2 = new FarmerPlan
        // {
        //     GardenId = garden1.ID,
        //     //ID = "plan1",
        //     Name = "Fridy plan",
        //     CronSchedule = "0/10 * * ? * FRI *"
        // };

        var plans = new FarmerPlan[] {
            plan0
        };

        _dbContext.Plans.AddRange(plans);

        _dbContext.SaveChanges();

        var planSteps = new List<FarmerPlanStep>() {
            // new FarmerPlanStep
            // {
            //     PlanId = plan0.ID, 
            //     TaskInterfaceFullName = typeof(IFarmerTakePictureTask).FullName
            // }
        };

        //planSteps.AddRange(new [] {
        //     new FarmerPlanStep
        //     {
        //         PlanId = plan0.ID, 
        //         BuildParameters = new Dictionary<string, string>() {{ nameof(IHasTargetHeight.TargetHeightInCm), "10.0" }}, 
        //         TaskInterfaceFullName = typeof(IFarmerMoveArmAtHeightTask).FullName // "SmartFarmer.Tasks.Movement.IFarmerMoveArmAtHeightTask"
        //     },
        //     new FarmerPlanStep
        //     {
        //         PlanId = plan0.ID, 
        //         BuildParameters = 
        //             new Dictionary<string, string>() 
        //             {
        //                 { nameof(IHasTargetGridPosition.TargetXInCm), "5.0" },
        //                 { nameof(IHasTargetGridPosition.TargetYInCm), "5.0" }
        //             },
        //         Delay = new System.TimeSpan(0, 0, 2),
        //         TaskClassFullName = typeof(IFarmerMoveOnGridTask).FullName // "SmartFarmer.Tasks.Movement.FarmerMoveOnGridTask"
        //     },
        //     new FarmerPlanStep
        //     {
        //         PlanId = plan0.ID, 
        //         BuildParameters = 
        //             new Dictionary<string, string>() 
        //             {
        //                 { nameof(IFarmerProvideWaterTask.PumpNumber), "1" },
        //                 { nameof(IFarmerProvideWaterTask.WaterAmountInLiters), "0.5" }
        //             },
        //         Delay = new System.TimeSpan(0, 0, 1),
        //         TaskInterfaceFullName = typeof(IFarmerProvideWaterTask).FullName // "SmartFarmer.Tasks.Irrigation.IFarmerProvideWaterTask"
        //     },
        //     new FarmerPlanStep
        //     {
        //         PlanId = plan0.ID, 
        //         BuildParameters = 
        //             new Dictionary<string, string>() 
        //             {
        //                 { nameof(IHasTargetGridPosition.TargetXInCm), "15.0" },
        //                 { nameof(IHasTargetGridPosition.TargetYInCm), "5.0" }
        //             },
        //         Delay = new System.TimeSpan(0, 0, 2),
        //         TaskClassFullName = typeof(IFarmerMoveOnGridTask).FullName // "SmartFarmer.Tasks.Movement.FarmerMoveOnGridTask"
        //     },
        //     new FarmerPlanStep
        //     {
        //         PlanId = plan0.ID, 
        //         BuildParameters = new Dictionary<string, string>() {{ nameof(IHasTargetHeight.TargetHeightInCm), "0.0" }},
        //         Delay = new System.TimeSpan(0, 0, 2),
        //         TaskInterfaceFullName = typeof(IFarmerMoveArmAtHeightTask).FullName // "SmartFarmer.Tasks.Movement.IFarmerMoveArmAtHeightTask"
        //     },
        // });


        // planSteps.Add(new FarmerPlanStep
        // {
        //     //ID = "p1s1",
        //     PlanId = plan1.ID,
        //     BuildParameters = new object[] { 5.0 },
        //     TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveArmAtHeightTask"
        // });

        // planSteps.Add(new FarmerPlanStep
        // {
        //     //ID = "p1s2",
        //     PlanId = plan2.ID,
        //     BuildParameters = new object[] { 15.0, 2.0 },
        //     TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveOnGridTask",
        //     Delay = new System.TimeSpan(0, 0, 5)
        // });

        // planSteps.Add(new FarmerPlanStep
        // {
        //     //ID = "p1s2",
        //     PlanId = plan2.ID,
        //     BuildParameters = new object[] { 3.0 },
        //     TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveArmAtHeightTask",
        // });

        // var alert = new FarmerAlert
        // {
        //     //ID = "alertId",
        //     FarmerGardenId = garden1.ID,
        //     Level = Alerts.AlertLevel.Warning,
        //     Severity = Alerts.AlertSeverity.Low
        // };

        _dbContext.Users.Add(user);
        _dbContext.Authorizations.Add(readGardenAuth);
        _dbContext.Authorizations.Add(editGardenAuth);
        _dbContext.Authorizations.Add(readUsersAuth);
        _dbContext.Authorizations.Add(editUsersAuth);

        _dbContext.PlantsInstance.AddRange(new [] {plantInstance1, plantInstance2});
        _dbContext.PlanSteps.AddRange(planSteps);

        // _dbContext.Alerts.Add(alert);

        _dbContext.SaveChanges();
    }
}