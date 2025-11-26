namespace FinanceManager.Api.Misc;

public abstract class AutoServiceRegistrationBase
{
    public abstract Task RegisterAsync(IServiceCollection services, IConfiguration configuration,
        IHostEnvironment hostEnvironment);

    public virtual int Priority => 1;
}