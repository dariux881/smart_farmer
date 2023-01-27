using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Alerts;
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

    }

    public DbSet<FarmerGround> Grounds { get; set; }
    public DbSet<FarmerPlant> Plants { get; set; }
    public DbSet<FarmerPlantInstance> PlantsInstance { get; set; }
    public DbSet<FarmerAlert> Alerts { get; set; }
    public DbSet<FarmerIrrigationTaskInfo> IrrigationInfo { get; set; }
    // public DbSet<FarmerPlan> PlantsInstance { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<UserLogin> Logins { get; set; }

}
