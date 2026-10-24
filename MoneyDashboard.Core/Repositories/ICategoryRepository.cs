using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;

namespace MoneyDashboard.Core.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetByTypeAsync(CategoryType categoryType);
}