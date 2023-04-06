using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Plants;

namespace SmartFarmer.Data.Configuration;

public class FarmerPlantTypeConfiguration : IEntityTypeConfiguration<FarmerPlant>
{
    public void Configure(EntityTypeBuilder<FarmerPlant> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
    }
}