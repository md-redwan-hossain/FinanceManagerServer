using FinanceManager.Api.Interfaces;

namespace FinanceManager.Api.Entities;

public class User: IAutoIncrementalEntity<long>
{
    public long Id { get; }
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public required string PasswordHash { get; set; }
}