using Microsoft.Extensions.Logging;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;

namespace MoneyDashboard.Core.Services;

public class AssetService : IAssetService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IMonthlyBankBalanceRepository _monthlyBankBalanceRepository;
    private readonly INisaRepository _nisaRepository;
    private readonly ILogger<AssetService> _logger;

    public AssetService(IBankAccountRepository bankAccountRepository, IMonthlyBankBalanceRepository monthlyBankBalanceRepository, INisaRepository nisaRepository, ILogger<AssetService> logger)
    {
        _bankAccountRepository = bankAccountRepository;
        _monthlyBankBalanceRepository = monthlyBankBalanceRepository;
        _nisaRepository = nisaRepository;
        _logger = logger;
    }

    public async Task<AssetData> GetAssetDataAsync(DateOnly targetMonth)
    {
        var bankAccounts = await _bankAccountRepository.GetAllAsync();

        var monthlyBalances = await _monthlyBankBalanceRepository.GetByMonthAsync(targetMonth);

        var nisa = await _nisaRepository.GetByMonthAsync(targetMonth);

        var activeAccountIds = bankAccounts
            .Where(account => account.IsActive)
            .Select(account => account.AccountId)
            .ToHashSet();

        var registeredBalanceAccountIds = monthlyBalances
            .Select(balance => balance.AccountId)
            .ToHashSet();

        var hasMissingBalance = activeAccountIds
            .Any(accountId => !registeredBalanceAccountIds.Contains(accountId));

        long? openingBankBalance;

        if (activeAccountIds.Count == 0 && monthlyBalances.Count == 0)
        {
            openingBankBalance = null;
        }
        else if (hasMissingBalance)
        {
            openingBankBalance = null;
        }
        else
        {
            openingBankBalance = monthlyBalances.Sum(balance => balance.OpeningBalance);
        }

        long? nisaEvaluationAmount = nisa?.EvaluationAmount;

        long? totalAssets = openingBankBalance.HasValue && nisaEvaluationAmount.HasValue ? openingBankBalance.Value + nisaEvaluationAmount.Value : null;

        return new AssetData
        {
            BankAccounts = bankAccounts,
            MonthlyBalances = monthlyBalances,
            OpeningBankBalance = openingBankBalance,
            NisaEvaluationAmount = nisaEvaluationAmount,
            TotalAssets = totalAssets
        };
    }

    public async Task<long> RegisterAccountAsync(BankAccount bankAccount)
    {
        var accountId = await _bankAccountRepository.InsertAsync(bankAccount);

        _logger.LogInformation(
                "Bank account registered. AccountId: {AccountId}",
                accountId);

        return accountId;
    }

    public async Task<bool> UpdateAccountAsync(long accountId, string accountName)
    {
        var result = await _bankAccountRepository.UpdateAsync(accountId, accountName);

        if (result)
        {
            _logger.LogInformation(
                "Bank account updated. AccountId: {AccountId}",
                accountId);
        }
        else
        {
            _logger.LogWarning(
                "Bank account update failed. AccountId: {AccountId}",
                accountId);
        }

        return result;
    }

    public async Task<bool> DeactivateAccountAsync(long accountId)
    {
        var result = await _bankAccountRepository.DeactivateAsync(accountId);

        if (result)
        {
            _logger.LogInformation(
                "Bank account deactivated. AccountId: {AccountId}",
                accountId);
        }
        else
        {
            _logger.LogWarning(
                "Bank account deactivation failed. AccountId: {AccountId}",
                accountId);
        }


        return result;
    }

    public async Task SaveOpeningBalanceAsync(long accountId, DateOnly targetMonth, long openingBalance)
    {
        await _monthlyBankBalanceRepository.UpsertAsync(
            accountId,
            targetMonth,
            openingBalance);

        _logger.LogInformation(
            "Opening bank balance saved. AccountId: {AccountId}, TargetMonth: {TargetMonth}",
            accountId,
            targetMonth.ToString("yyyy-MM"));
    }

    public async Task SaveNisaValueAsync(DateOnly targetMonth, long evaluationAmount)
    {
        await _nisaRepository.UpsertAsync(
                targetMonth,
                evaluationAmount);

        _logger.LogInformation(
            "NISA value saved. TargetMonth: {TargetMonth}",
            targetMonth.ToString("yyyy-MM"));
    }
}