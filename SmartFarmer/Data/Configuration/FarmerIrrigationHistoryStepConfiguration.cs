using SmartFarmer.DTOs.Plants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartFarmer.Data.Configuration;

public class FarmerIrrigationHistoryStepConfiguration : IEntityTypeConfiguration<IrrigationHistoryStep>
{
    public void Configure(EntityTypeBuilder<IrrigationHistoryStep> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();

    }
}