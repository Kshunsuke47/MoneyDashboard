using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetByMonthAsync(DateOnly targetMonth);

    Task<Expense?> GetByIdAsync(long expenseId);

    Task<long> GetTotalByMonthAsync(DateOnly targetMonth);

    Task<IReadOnlyList<Expense>> GetRecentUnexpectedAsync(
        DateOnly targetMonth,
        int limit);

    Task<long> InsertAsync(Expense expense);

    Task<bool> UpdateAsync(Expense expense);

    Task<bool> DeleteAsync(long expenseId);
}