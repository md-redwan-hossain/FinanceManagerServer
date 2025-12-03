using FinanceManager.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.Api.Persistence.EntityConfigs;

public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(x => x.NormalizedTitle).IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(50);

        builder.Property(x => x.NormalizedTitle)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);
    }
}