using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace FinanceManager.Api.Misc;

public static class AppExtensions
{
    extension(MvcOptions opts)
    {
        public void UseGlobalRoutePrefix(string routeTemplate)
        {
            opts.Conventions.Insert(0, new GlobalRoutePrefixConvention(new RouteAttribute(routeTemplate)));
        }

        public void UseGlobalRoutePrefix(IRouteTemplateProvider routeAttribute)
        {
            opts.Conventions.Insert(0, new GlobalRoutePrefixConvention(routeAttribute));
        }
    }

    extension(IServiceCollection services)
    {
        public IServiceCollection BindAndValidateOptions<TOptions>(string sectionName) where TOptions : class
        {
            services.AddOptions<TOptions>()
                .BindConfiguration(sectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();
            return services;
        }

        public async Task ExecuteAutoServiceRegistrationAsync(
            IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            var intendedName = nameof(AutoServiceRegistrationBase).Replace("Base", string.Empty);

            // Force load all module assemblies
            var allAssemblies = LoadAllModuleAssemblies(intendedName);

            // Collect all registration modules
            var registrationModules = new List<AutoServiceRegistrationBase>();

            foreach (var assembly in allAssemblies)
            {
                var moduleTypes = assembly
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false } &&
                                typeof(AutoServiceRegistrationBase).IsAssignableFrom(t));

                foreach (var moduleType in moduleTypes)
                {
                    // Validate naming convention
                    ValidateNamingConvention(moduleType, intendedName);

                    // Validate sealed requirement
                    ValidateSealedRequirement(moduleType);

                    if (Activator.CreateInstance(moduleType) is AutoServiceRegistrationBase module)
                    {
                        registrationModules.Add(module);
                    }
                }
            }

            // Register modules in priority order
            foreach (var module in registrationModules.OrderByDescending(m => m.Priority))
            {
                await module.RegisterAsync(services, configuration, hostEnvironment);
            }
        }
    }


    private static void ValidateNamingConvention(Type moduleType, string intendedName)
    {
        if (moduleType.Name != intendedName)
        {
            throw new InvalidOperationException(
                $"{moduleType.FullName} inherits from {nameof(AutoServiceRegistrationBase)} but is not named {intendedName}");
        }
    }

    private static void ValidateSealedRequirement(Type moduleType)
    {
        if (!moduleType.IsSealed)
        {
            throw new InvalidOperationException(
                $"{moduleType.FullName} inherits from {nameof(AutoServiceRegistrationBase)} but is not sealed");
        }
    }

    private static List<Assembly> LoadAllModuleAssemblies(string intendedName)
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = loadedAssemblies.Select(a => a.Location).ToArray();

        // Get all DLL files in the application directory
        var referencedPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

        // Load assemblies that contain AutoServiceRegistrationBase implementations
        var toLoad = referencedPaths
            .Where(r => !loadedPaths.Contains(r, StringComparer.InvariantCultureIgnoreCase))
            .ToList();

        foreach (var path in toLoad)
        {
            try
            {
                var assembly = Assembly.LoadFrom(path);
                if (AssemblyContainsValidAutoServiceRegistration(assembly, intendedName))
                {
                    if (!loadedAssemblies.Contains(assembly))
                    {
                        loadedAssemblies.Add(assembly);
                    }
                }
            }
            catch
            {
                // Ignore assemblies that can't be loaded
            }
        }

        return loadedAssemblies.Where(a => !a.IsDynamic).ToList();
    }

    private static bool AssemblyContainsValidAutoServiceRegistration(Assembly assembly, string intendedName)
    {
        try
        {
            return assembly.GetTypes()
                .Any(t => t is { IsClass: true, IsAbstract: false, IsSealed: true } &&
                          typeof(AutoServiceRegistrationBase).IsAssignableFrom(t) &&
                          t.Name == intendedName);
        }
        catch
        {
            return false;
        }
    }
}