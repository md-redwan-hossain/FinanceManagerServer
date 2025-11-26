using System.ComponentModel.DataAnnotations;

namespace FinanceManager.Api;

public record AppOptions
{
    public const string SectionName = "APP_OPTIONS";

    [Required]
    [ConfigurationKeyName("ALLOWED_ORIGINS_FOR_CORS")]
    public required string[] AllowedOriginsForCors { get; init; }

    [Required]
    [ConfigurationKeyName("APP_DB")]
    public required string AppDb { get; init; }
}