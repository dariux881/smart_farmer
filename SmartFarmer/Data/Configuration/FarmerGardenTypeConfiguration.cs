using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs;

namespace SmartFarmer.Data.Configuration;

public class FarmerGardenTypeConfiguration : IEntityTypeConfiguration<FarmerGarden>
{
    public void Configure(EntityTypeBuilder<FarmerGarden> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
            
        builder
            .Property(p => p.LengthInMeters)
            .IsRequired();

        builder
            .Property(p => p.WidthInMeters)
            .IsRequired();

        builder
            .Property(p => p.Latitude)
            .IsRequired();

        builder
            .Property(p => p.Longitude)
            .IsRequired();
            
        builder
            .Property(p => p.UserID)
            .IsRequired();
    }
}