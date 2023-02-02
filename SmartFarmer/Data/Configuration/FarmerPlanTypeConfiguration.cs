using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.Data.Configuration;

public class FarmerPlanTypeConfiguration : IEntityTypeConfiguration<FarmerPlan>
{
    public void Configure(EntityTypeBuilder<FarmerPlan> builder)
    {
        builder
            .HasOne(p => p.Ground)
            .WithMany(g => g.Plans)
            .HasForeignKey("GroundId");
        
        builder
            .Ignore(s => s.IsInProgress);
        
        builder
            .Ignore(s => s.LastException);

    }
}