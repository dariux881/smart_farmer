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
        Task.Run(async () => await PopulateDB());
    }

    private async Task PopulateDB()
    {
        if (populated) return;
        populated = true;

        var user = new User { ID = "user0", UserName="test", Password="test", Email="prova@test.it"};

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

        var plant1 = new FarmerPlant { ID = "plant1", FriendlyName="plant 1", IrrigationInfoId = irrInfo1.ID };
        var plant2 = new FarmerPlant { ID = "plant2", FriendlyName="plant 2", IrrigationInfoId = irrInfo2.ID };

        var plantInstance1 = new FarmerPlantInstance { ID = "plant1", PlantKindID=plant1.ID, PlantName=plant1.FriendlyName, PlantX=1, PlantY=2 };

        _dbContext.Users.Add(user);
        _dbContext.IrrigationInfo.AddRange(new [] {irrInfo1, irrInfo2 });
        _dbContext.Plants.AddRange(new [] {plant1, plant2});
        _dbContext.PlantsInstance.Add(plantInstance1);

        await _dbContext.SaveChangesAsync();

        var ground1 = new DTOs.FarmerGround(this)
            { 
                ID = "gID", 
                UserID="user0", 
                GroundName="Ground Name",                
            };

        await ground1.AddPlantAsync(plantInstance1.ID);

        _dbContext.Grounds.Add( ground1);

        await _dbContext.SaveChangesAsync();
    }
}