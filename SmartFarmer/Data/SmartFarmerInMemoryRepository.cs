using System.Threading.Tasks;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;

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

        var readGroundAuth = new Authorization { ID = "readGround", Name="Read Ground" };
        var editGroundAuth = new Authorization { ID = "editGround", Name="Change Ground adding plans and plants" };
        var readUsersAuth = new Authorization { ID = "readUsers", Name="See current users" };
        var editUsersAuth = new Authorization { ID = "editUsers", Name="Edit users, creating or deleting them, changing their permissions" };

        user.Authorizations.Add(readGroundAuth);


        var irrInfo1 = new FarmerIrrigationTaskInfo {
            ID = "ii0",
            AmountOfWaterInLitersPerTime = 1,
            TimesPerWeek = 1
        };
        var irrInfo2 = new FarmerIrrigationTaskInfo {
            ID = "ii1",
            AmountOfWaterInLitersPerTime = 5,
            TimesPerWeek = 1
        };

        var plant1 = new FarmerPlant { 
            ID = "plant1", 
            Code = "123",
            FriendlyName="plant 1", 
            IrrigationInfoId = irrInfo1.ID };
        var plant2 = new FarmerPlant { 
            ID = "plant2", 
            Code = "124",
            FriendlyName="plant 2", 
            IrrigationInfoId = irrInfo2.ID };

        var ground1 = new DTOs.FarmerGround(this)
        { 
            ID = "gID", 
            UserID="user0", 
            GroundName="Ground Name",
        };

        var plantInstance1 = new FarmerPlantInstance { 
            ID = "P1", 
            PlantKindID=plant1.ID, 
            PlantName=plant1.FriendlyName, 
            PlantX=1, 
            PlantY=2,
            FarmerGroundId = ground1.ID 
        };

        var plantInstance2 = new FarmerPlantInstance { 
            ID = "P2", 
            PlantKindID=plant2.ID, 
            PlantName="pianta", 
            PlantX=10, 
            PlantY=22,
            FarmerGroundId = ground1.ID 
        };

        var plan1 = new FarmerPlan
        {
            GroundId = ground1.ID,
            ID = "plan1",
            Name = "example plan"
        };

        _dbContext.Users.Add(user);
        _dbContext.Authorizations.Add(readGroundAuth);
        _dbContext.Authorizations.Add(editGroundAuth);
        _dbContext.Authorizations.Add(readUsersAuth);
        _dbContext.Authorizations.Add(editUsersAuth);

        _dbContext.IrrigationInfo.AddRange(new [] {irrInfo1, irrInfo2 });
        _dbContext.Plants.AddRange(new [] {plant1, plant2});
        _dbContext.PlantsInstance.AddRange(new [] {plantInstance1, plantInstance2});
        _dbContext.Plans.Add(plan1);

        _dbContext.SaveChanges();

        ground1.AddPlants( new [] { plantInstance1, plantInstance2 } );
        ground1.AddPlan(plan1);

        _dbContext.Grounds.Add(ground1);

        _dbContext.SaveChanges();
    }
}