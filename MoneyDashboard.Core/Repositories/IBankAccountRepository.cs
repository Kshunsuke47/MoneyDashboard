using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface IBankAccountRepository
{
    Task<IReadOnlyList<BankAccount>> GetAllAsync();

    Task<BankAccount?> GetByIdAsync(long accountId);

    Task<long> InsertAsync(BankAccount bankAccount);

    Task<bool> UpdateAsync(long accountId, string accountName);

    Task<bool> DeactivateAsync(long accountId);
}