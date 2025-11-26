using FinanceManager.Api.Misc;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Api.Persistence;

public sealed class AutoServiceRegistration : AutoServiceRegistrationBase
{
    public override Task RegisterAsync(IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        var appOptions = configuration.GetRequiredSection(AppOptions.SectionName)
            .Get<AppOptions>();

        ArgumentNullException.ThrowIfNull(appOptions);

        services.AddDbContext<AppDbContext>(opts =>
        {
            if (hostEnvironment.IsDevelopment())
            {
                opts.LogTo(Console.WriteLine, LogLevel.Information);
                opts.EnableSensitiveDataLogging();
                opts.EnableDetailedErrors();
            }

            if (hostEnvironment.IsProduction())
            {
                opts.LogTo(Console.WriteLine, LogLevel.Warning);
            }

            opts.UseNpgsql(appOptions.AppDb);
        });

        return Task.CompletedTask;
    }
}