using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Tasks;

namespace SmartFarmer.Data.Configuration;

public class FarmerPlanStepTypeConfiguration : IEntityTypeConfiguration<FarmerPlanStep>
{
    public void Configure(EntityTypeBuilder<FarmerPlanStep> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
            
        builder
            .HasOne(s => s.Plan)
            .WithMany(p => p.Steps)
            .HasForeignKey("PlanId");
        
        builder
            .Ignore(s => s.BuildParameters)
            .Ignore(s => s.IsInProgress)
            .Ignore(s => s.LastException);
    }
}