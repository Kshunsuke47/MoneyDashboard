using Microsoft.Extensions.Logging;
using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;

namespace MoneyDashboard.Core.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ILogger<SubscriptionService> _logger;
    private readonly ICategoryRepository _categoryRepository;

    public SubscriptionService(ISubscriptionRepository subscriptionRepository, ICategoryRepository categoryRepository, ILogger<SubscriptionService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Subscription>> GetAllAsync()
    {
        return _subscriptionRepository.GetAllAsync();
    }

    public Task<Subscription?> GetByIdAsync(long subscriptionId)
    {
        return _subscriptionRepository.GetByIdAsync(subscriptionId);
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        return _categoryRepository.GetByTypeAsync(
            CategoryType.Subscription);
    }

    public async Task<long> RegisterAsync(Subscription subscription)
    {
        var subscriptionId = await _subscriptionRepository.InsertAsync(subscription);

        _logger.LogInformation("Subscription registered. SubscriptionId: {SubscriptionId}", subscriptionId);

        return subscriptionId;
    }

    public async Task<bool> UpdateAsync(Subscription subscription)
    {
        var result = await _subscriptionRepository.UpdateAsync(subscription);

        if (result)
        {
            _logger.LogInformation(
                "Subscription updated. SubscriptionId: {SubscriptionId}",
                subscription.SubscriptionId);
        }
        else
        {
            _logger.LogWarning(
                "Subscription update failed. SubscriptionId: {SubscriptionId}",
                subscription.SubscriptionId);
        }

        return result;
    }

    public async Task<bool> DeleteAsync(long subscriptionId)
    {
        var result = await _subscriptionRepository.DeleteAsync(subscriptionId);

        if (result)
        {
            _logger.LogInformation(
                "Subscription deleted. SubscriptionId: {SubscriptionId}",
                subscriptionId);
        }
        else
        {
            _logger.LogWarning(
                "Subscription delete failed. SubscriptionId: {SubscriptionId}",
                subscriptionId);
        }

        return result;
    }

    public long CalculateMonthlyAmount(Subscription subscription)
    {
        if (subscription.PaymentCycle == PaymentCycle.Monthly)
        {
            return subscription.PaymentAmount;
        }

        var monthlyAmount = Math.Round(subscription.PaymentAmount / 12m, 0, MidpointRounding.AwayFromZero);

        return (long)monthlyAmount;
    }

    public async Task<long> GetActiveMonthlyTotalAsync()
    {
        var subscriptions =
            await _subscriptionRepository.GetActiveAsync();

        return subscriptions.Sum(
            subscription =>
                CalculateMonthlyAmount(subscription));
    }

    public async Task<int> GetActiveCountAsync()
    {
        var subscriptions = await _subscriptionRepository.GetActiveAsync();

        return subscriptions.Count();
    }
}