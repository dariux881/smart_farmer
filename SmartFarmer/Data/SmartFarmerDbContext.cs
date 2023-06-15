using Microsoft.EntityFrameworkCore;
using SmartFarmer.Data.Configuration;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Alerts;
using SmartFarmer.DTOs.Movements;
using SmartFarmer.DTOs.Plants;
using SmartFarmer.DTOs.Security;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.Data;

public class SmartFarmerDbContext : DbContext
{
    public SmartFarmerDbContext(DbContextOptions<SmartFarmerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurations 
        new FarmerGardenTypeConfiguration().Configure(modelBuilder.Entity<FarmerGarden>());
        new FarmerDevicePositionTypeConfiguration().Configure(modelBuilder.Entity<FarmerDevicePosition>());
        new FarmerPlantTypeConfiguration().Configure(modelBuilder.Entity<FarmerPlant>());
        new FarmerPlantInstanceTypeConfiguration().Configure(modelBuilder.Entity<FarmerPlantInstance>());
        new FarmerPlanTypeConfiguration().Configure(modelBuilder.Entity<FarmerPlan>());
        new FarmerPlanStepTypeConfiguration().Configure(modelBuilder.Entity<FarmerPlanStep>());
        new FarmerAlertTypeConfiguration().Configure(modelBuilder.Entity<FarmerAlert>());
        new FarmerIrrigationHistoryStepConfiguration().Configure(modelBuilder.Entity<IrrigationHistoryStep>());

        new FarmerUserTypeConfiguration().Configure(modelBuilder.Entity<User>());
        new FarmerUserLoginTypeConfiguration().Configure(modelBuilder.Entity<UserLogin>());
    }

    public DbSet<FarmerGarden> Gardens { get; set; }
    public DbSet<FarmerPlant> Plants { get; set; }
    public DbSet<FarmerPlan> Plans { get; set; }
    public DbSet<FarmerPlanStep> PlanSteps { get; set; }
    public DbSet<FarmerPlantInstance> PlantsInstance { get; set; }
    public DbSet<FarmerAlert> Alerts { get; set; }
    public DbSet<IrrigationHistoryStep> IrrigationHistory { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<UserLogin> Logins { get; set; }
    public DbSet<Authorization> Authorizations { get; set; }
    public DbSet<FarmerDevicePosition> DevicePositions { get; set; }
}
