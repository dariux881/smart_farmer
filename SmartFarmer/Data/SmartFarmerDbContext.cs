using System.Data.Entity;
using SmartFarmer.DTOs;
using SmartFarmer.DTOs.Alerts;

namespace SmartFarmer.Data;

public class SmartFarmerDbContext : DbContext
{
    public DbSet<FarmerGround> Grounds { get; set; }
    public DbSet<FarmerAlert> Alerts { get; set; }
}
