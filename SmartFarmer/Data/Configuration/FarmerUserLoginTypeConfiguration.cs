using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFarmer.DTOs.Security;

namespace SmartFarmer.Data.Configuration;

public class FarmerUserLoginTypeConfiguration : IEntityTypeConfiguration<UserLogin>
{
    public void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        builder
            .Property(f => f.ID)
            .ValueGeneratedOnAdd();
    }
}