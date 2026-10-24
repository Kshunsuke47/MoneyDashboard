using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface ISubscriptionRepository
{
    Task<IReadOnlyList<Subscription>> GetAllAsync();

    Task<IReadOnlyList<Subscription>> GetActiveAsync();

    Task<Subscription?> GetByIdAsync(long subscriptionId);

    Task<long> InsertAsync(Subscription subscription);

    Task<bool> UpdateAsync(Subscription subscription);

    Task<bool> DeleteAsync(long subscriptionId);
}