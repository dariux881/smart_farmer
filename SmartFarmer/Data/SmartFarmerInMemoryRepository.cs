using System.Threading.Tasks;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;
using SmartFarmer.Helpers;

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

        var readGroundAuth = new Authorization { ID = Constants.AUTH_READ_GROUND, Name="Read Ground" };
        var editGroundAuth = new Authorization { ID = Constants.AUTH_EDIT_GROUND, Name="Change Ground adding plans and plants" };
        var readUsersAuth = new Authorization { ID = Constants.AUTH_READ_USERS, Name="See current users" };
        var editUsersAuth = new Authorization { ID = Constants.AUTH_EDIT_USERS, Name="Edit users, creating or deleting them, changing their permissions" };

        user.Authorizations.Add(readGroundAuth);

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

        _dbContext.IrrigationInfo.AddRange(new [] {irrInfo1, irrInfo2 });
        _dbContext.SaveChanges();

        var plant1 = new FarmerPlant { 
            //ID = "plant1", 
            Code = "123",
            FriendlyName="plant 1", 
            IrrigationInfoId = irrInfo1.ID };
        var plant2 = new FarmerPlant { 
            //ID = "plant2", 
            Code = "124",
            FriendlyName="plant 2", 
            IrrigationInfoId = irrInfo2.ID };

        var ground1 = new DTOs.FarmerGround(this)
        { 
            //ID = "gID", 
            UserID="user0", 
            GroundName="Ground Name",
        };

        _dbContext.Grounds.Add(ground1);
        _dbContext.Plants.AddRange(new [] {plant1, plant2});
        _dbContext.SaveChanges();

        var plantInstance1 = new FarmerPlantInstance { 
            //ID = "P1", 
            PlantKindID=plant1.ID, 
            PlantName=plant1.FriendlyName, 
            PlantX=1, 
            PlantY=2,
            FarmerGroundId = ground1.ID 
        };

        var plantInstance2 = new FarmerPlantInstance { 
            //ID = "P2", 
            PlantKindID=plant2.ID, 
            PlantName="pianta", 
            PlantX=10, 
            PlantY=22,
            FarmerGroundId = ground1.ID 
        };

        var plan1 = new FarmerPlan
        {
            GroundId = ground1.ID,
            //ID = "plan1",
            Name = "Monday plan",
            FarmerDaysMask = "0100000"
        };

        var plan2 = new FarmerPlan
        {
            GroundId = ground1.ID,
            //ID = "plan1",
            Name = "Tuesday plan",
            FarmerDaysMask = "0010000"
        };

        _dbContext.Plans.Add(plan1);
        _dbContext.Plans.Add(plan2);
        _dbContext.SaveChanges();

        var p1Step1 = new FarmerPlanStep
        {
            //ID = "p1s1",
            PlanId = plan1.ID,
            BuildParameters = new object[] { 5.0 },
            TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveArmAtHeight"
        };
        var p2Step1 = new FarmerPlanStep
        {
            //ID = "p1s2",
            PlanId = plan2.ID,
            BuildParameters = new object[] { 15.0, 2.0 },
            TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveOnGrid",
            Delay = new System.TimeSpan(0, 0, 5)
        };
        var p2Step2 = new FarmerPlanStep
        {
            //ID = "p1s2",
            PlanId = plan2.ID,
            BuildParameters = new object[] { 3.0 },
            TaskClassFullName = "SmartFarmer.Tasks.Movement.FarmerMoveArmAtHeight",
        };

        var alert = new FarmerAlert
        {
            //ID = "alertId",
            FarmerGroundId = ground1.ID,
            Level = Alerts.AlertLevel.Warning,
            Severity = Alerts.AlertSeverity.Low
        };

        _dbContext.Users.Add(user);
        _dbContext.Authorizations.Add(readGroundAuth);
        _dbContext.Authorizations.Add(editGroundAuth);
        _dbContext.Authorizations.Add(readUsersAuth);
        _dbContext.Authorizations.Add(editUsersAuth);

        _dbContext.PlantsInstance.AddRange(new [] {plantInstance1, plantInstance2});
        _dbContext.PlanSteps.Add(p1Step1);
        _dbContext.PlanSteps.Add(p2Step1);
        _dbContext.PlanSteps.Add(p2Step2);

        _dbContext.Alerts.Add(alert);

        _dbContext.SaveChanges();
    }
}