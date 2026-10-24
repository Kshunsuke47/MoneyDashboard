using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();

    Task<IReadOnlyList<Expense>> GetByMonthAsync(DateOnly targetMonth);

    Task<Expense?> GetByIdAsync(long expenseId);

    Task<long> GetTotalByMonthAsync(DateOnly targetMonth);

    Task<IReadOnlyList<Expense>> GetRecentUnexpectedAsync(
        DateOnly targetMonth,
        int limit);

    Task<long> RegisterAsync(Expense expense);

    Task<bool> UpdateAsync(Expense expense);

    Task<bool> DeleteAsync(long expenseId);
}