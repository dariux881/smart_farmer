using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Alerts;

namespace SmartFarmer.Data.Configuration;

public class FarmerAlertTypeConfiguration : IEntityTypeConfiguration<FarmerAlert>
{
    public void Configure(EntityTypeBuilder<FarmerAlert> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
            
        builder
            .HasOne(s => s.FarmerGround)
            .WithMany(p => p.Alerts)
            .HasForeignKey("FarmerGroundId");
        
        builder
            .HasOne(s => s.PlantInstance)
            .WithMany()
            .HasForeignKey("PlantInstanceId");
        
        builder
            .HasOne(s => s.RaisedByTask)
            .WithMany()
            .HasForeignKey("RaisedByTaskId");
    }
}