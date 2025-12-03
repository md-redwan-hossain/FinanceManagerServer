namespace FinanceManager.Api.DataTransferObjects;

public record CategoryRequest
{
    public required string Title { get; init; }
    public required string NormalizedTitle { get; init; }
    public required string? Description { get; init; }
}