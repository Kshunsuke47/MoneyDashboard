using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Services;

public interface IAssetService
{
    Task<AssetData> GetAssetDataAsync(DateOnly targetMonth);

    Task<long> RegisterAccountAsync(BankAccount bankAccount);

    Task<bool> UpdateAccountAsync(
        long accountId,
        string accountName);

    Task<bool> DeactivateAccountAsync(long accountId);

    Task SaveOpeningBalanceAsync(
        long accountId,
        DateOnly targetMonth,
        long openingBalance);

    Task SaveNisaValueAsync(
        DateOnly targetMonth,
        long evaluationAmount);
}