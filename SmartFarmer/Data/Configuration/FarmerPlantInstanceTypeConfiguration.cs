using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Data.Configuration;

public class FarmerPlantInstanceTypeConfiguration : IEntityTypeConfiguration<FarmerPlantInstance>
{
    public void Configure(EntityTypeBuilder<FarmerPlantInstance> builder)
    {
        builder
            .Property(p => p.PlantKindID)
            .IsRequired();

        builder
            .Property(p => p.FarmerGroundId)
            .IsRequired();

        builder
            .Property(p => p.PlantX)
            .IsRequired();

        builder
            .Property(p => p.PlantY)
            .IsRequired();
            
        builder
            .Property(p => p.PlantedWhen)
            .IsRequired();
            
        builder
            .HasOne(p => p.Plant);
    }
}