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
            .HasOne(s => s.FarmerGarden)
            .WithMany(p => p.Alerts)
            .HasForeignKey(nameof(FarmerAlert.FarmerGardenId));
        
        builder
            .HasOne(s => s.PlantInstance)
            .WithMany()
            .HasForeignKey(nameof(FarmerAlert.PlantInstanceId));
        
        builder
            .HasOne(s => s.RaisedByTask)
            .WithMany()
            .HasForeignKey(nameof(FarmerAlert.RaisedByTaskId));
    }
}