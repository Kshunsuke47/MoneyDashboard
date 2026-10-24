using Microsoft.Extensions.Logging;
using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;

namespace MoneyDashboard.Core.Services;

public class IncomeService : IIncomeService
{
    private readonly IIncomeRepository _incomeRepository;
    private readonly ILogger<IncomeService> _logger;
    private readonly ICategoryRepository _categoryRepository;

    public IncomeService(IIncomeRepository incomeRepository, ICategoryRepository categoryRepository, ILogger<IncomeService> logger)
    {
        _incomeRepository = incomeRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        return _categoryRepository.GetByTypeAsync(
            CategoryType.Income);
    }

    public Task<IReadOnlyList<Income>> GetByMonthAsync(DateOnly targetMonth)
    {
        return _incomeRepository.GetByMonthAsync(targetMonth);
    }

    public Task<Income?> GetByIdAsync(long incomeId)
    {
        return _incomeRepository.GetByIdAsync(incomeId);
    }

    public Task<long> GetTotalByMonthAsync(DateOnly targetMonth)
    {
        return _incomeRepository.GetTotalByMonthAsync(targetMonth);
    }

    public async Task<long> RegisterAsync(Income income)
    {
        var incomeId = await _incomeRepository.InsertAsync(income);

        _logger.LogInformation("Income registered. IncomeId: {IncomeId}", incomeId);
        return incomeId;
    }

    public async Task<bool> UpdateAsync(Income income)
    {
        var result = await _incomeRepository.UpdateAsync(income);

        if (result)
        {
            _logger.LogInformation(
                "Income updated. IncomeId: {IncomeId}",
                income.IncomeId);
        }
        else
        {
            _logger.LogWarning(
                "Income update failed. IncomeId: {IncomeId}",
                income.IncomeId);
        }

        return result;
    }

    public async Task<bool> DeleteAsync(long incomeId)
    {
        var result = await _incomeRepository.DeleteAsync(incomeId);

        if (result)
        {
            _logger.LogInformation(
                "Income deleted. IncomeId: {IncomeId}",
                incomeId);
        }
        else
        {
            _logger.LogWarning(
                "Income delete failed. IncomeId: {IncomeId}",
                incomeId);
        }

        return result;
    }
}