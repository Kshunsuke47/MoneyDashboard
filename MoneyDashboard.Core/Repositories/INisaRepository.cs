using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface INisaRepository
{
    Task<MonthlyNisaValue?> GetByMonthAsync(DateOnly targetMonth);

    Task<bool> UpsertAsync(
        DateOnly targetMonth,
        long evaluationAmount);
}