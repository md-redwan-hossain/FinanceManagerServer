using FinanceManager.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceManager.Api.Persistence.EntityConfigs;

public class ItemConfig : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasIndex(x => x.NormalizedTitle).IsUnique();

        builder.Property(x => x.Title)
            .HasMaxLength(50);

        builder.Property(x => x.NormalizedTitle)
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}