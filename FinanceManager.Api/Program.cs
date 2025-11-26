using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using DotNetEnv;
using FinanceManager.Api;
using FinanceManager.Api.Misc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using SharpServiceCollection.Extensions;

Env.Load();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables();
    
    builder.Services.BindAndValidateOptions<AppOptions>(AppOptions.SectionName);
    await builder.Services.ExecuteAutoServiceRegistrationAsync(builder.Configuration, builder.Environment);

    builder.Services.AddSerilog((_, lc) => lc
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(configuration)
    );

    if (builder.Environment.IsDevelopment())
    {
        SelfLog.Enable(Console.Error);
    }
    
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1);
        options.ReportApiVersions = true;
    });

    builder.Services.AddControllers(opts =>
            {
                opts.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
                opts.UseGlobalRoutePrefix("api/v{version:apiVersion}");
                opts.OutputFormatters.RemoveType<StringOutputFormatter>();
                opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
            }
        )
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
        });


    var appOptions = builder.Configuration.GetRequiredSection(AppOptions.SectionName).Get<AppOptions>();
    ArgumentNullException.ThrowIfNull(appOptions);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(nameof(AppOptions.AllowedOriginsForCors), x => x
            .WithOrigins(appOptions.AllowedOriginsForCors)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
        );
    });


    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddOpenApi();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddServicesFromCurrentAssembly();

    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Theme = ScalarTheme.BluePlanet;
            options.ShowSidebar = true;
        });
    }

    app.UseSecurityHeaders();
    app.UseCors(nameof(AppOptions.AllowedOriginsForCors));
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseExceptionHandler();
    app.MapControllers();

    Console.WriteLine("Application started");
    await app.RunAsync();

    Console.WriteLine("");
    Console.WriteLine("Shut down complete");

    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application start-up failed");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}