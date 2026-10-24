using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Services;

public interface IIncomeService
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync();

    Task<IReadOnlyList<Income>> GetByMonthAsync(DateOnly targetMonth);

    Task<Income?> GetByIdAsync(long incomeId);

    Task<long> GetTotalByMonthAsync(DateOnly targetMonth);

    Task<long> RegisterAsync(Income income);

    Task<bool> UpdateAsync(Income income);

    Task<bool> DeleteAsync(long incomeId);
}