using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Services;

public interface ISubscriptionService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();

    Task<IReadOnlyList<Subscription>> GetAllAsync();

    Task<Subscription?> GetByIdAsync(long subscriptionId);

    Task<long> RegisterAsync(Subscription subscription);

    Task<bool> UpdateAsync(Subscription subscription);

    Task<bool> DeleteAsync(long subscriptionId);

    long CalculateMonthlyAmount(Subscription subscription);

    Task<long> GetActiveMonthlyTotalAsync();

    Task<int> GetActiveCountAsync();
}