using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.Data.Configuration;

public class FarmerIrrigationTaskInfoConfiguration : IEntityTypeConfiguration<FarmerIrrigationTaskInfo>
{
    public void Configure(EntityTypeBuilder<FarmerIrrigationTaskInfo> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();

    }
}