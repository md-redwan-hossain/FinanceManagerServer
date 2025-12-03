using FinanceManager.Api.Interfaces;

namespace FinanceManager.Api.Entities;

public class Category : IAutoIncrementalEntity<long>
{
    public long Id { get; }
    public required string Title { get; set; }
    public required string NormalizedTitle { get; set; }
    public required string? Description { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime? UpdatedAt { get; set; }
}