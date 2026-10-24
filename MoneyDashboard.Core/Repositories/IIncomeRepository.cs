using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface IIncomeRepository
{
    Task<IReadOnlyList<Income>> GetByMonthAsync(DateOnly targetMonth);

    Task<Income?> GetByIdAsync(long incomeId);

    Task<long> GetTotalByMonthAsync(DateOnly targetMonth);

    Task<long> InsertAsync(Income income);

    Task<bool> UpdateAsync(Income income);

    Task<bool> DeleteAsync(long incomeId);
}
