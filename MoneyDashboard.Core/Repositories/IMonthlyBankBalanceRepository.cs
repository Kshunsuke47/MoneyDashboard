using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface IMonthlyBankBalanceRepository
{
    Task<IReadOnlyList<MonthlyBankBalance>> GetByMonthAsync(
        DateOnly targetMonth);

    Task<MonthlyBankBalance?> GetByAccountAndMonthAsync(
        long accountId,
        DateOnly targetMonth);

    Task<long> GetTotalByMonthAsync(DateOnly targetMonth);

    Task<bool> UpsertAsync(
        long accountId,
        DateOnly targetMonth,
        long openingBalance);
}