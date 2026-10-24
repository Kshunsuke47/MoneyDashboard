using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Services;

public interface IDashboardService
{
    Task<DashboardData> GetDashboardAsync(DateOnly targetMonth);
}