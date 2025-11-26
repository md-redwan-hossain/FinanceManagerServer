using FinanceManager.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.Api.Persistence.EntityConfigs;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(x => x.UserName).IsUnique();

        builder.Property(x => x.UserName)
            .HasMaxLength(20);

        builder.Property(x => x.FullName)
            .HasMaxLength(50);
    }
}