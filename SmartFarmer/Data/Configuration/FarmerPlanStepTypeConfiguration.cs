using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.Data.Configuration;

public class FarmerPlanStepTypeConfiguration : IEntityTypeConfiguration<FarmerPlanStep>
{
    public void Configure(EntityTypeBuilder<FarmerPlanStep> builder)
    {
        builder
            .HasOne(s => s.Plan)
            .WithMany(p => p.Steps)
            .HasForeignKey("PlanId");
        
        builder
            .Ignore(s => s.BuildParameters);
        
        builder
            .Ignore(s => s.BuildParametersString);
            
        builder
            .Ignore(s => s.IsInProgress);
        
        builder
            .Ignore(s => s.LastException);
    }
}