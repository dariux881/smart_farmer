using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Movements;

namespace SmartFarmer.Data.Configuration;

public class FarmerDevicePositionTypeConfiguration : IEntityTypeConfiguration<FarmerDevicePosition>
{
    public void Configure(EntityTypeBuilder<FarmerDevicePosition> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
            
        builder
            .Property(p => p.GroundId)
            .IsRequired();
            
        builder
            .Property(p => p.UserId)
            .IsRequired();
            
        builder
            .HasOne(p => p.Ground)
            .WithMany()
            .HasForeignKey(nameof(FarmerDevicePosition.GroundId));
    }
}
