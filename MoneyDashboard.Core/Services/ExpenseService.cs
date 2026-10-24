using Microsoft.Extensions.Logging;
using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;

namespace MoneyDashboard.Core.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(IExpenseRepository expenseRepository, ICategoryRepository categoryRepository, ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public Task<IReadOnlyList<Expense>> GetByMonthAsync(DateOnly targetMonth)
    {
        return _expenseRepository.GetByMonthAsync(targetMonth);
    }

    public Task<Expense?> GetByIdAsync(long expenseId)
    {
        return _expenseRepository.GetByIdAsync(expenseId);
    }

    public Task<long> GetTotalByMonthAsync(DateOnly targetMonth)
    {
        return _expenseRepository.GetTotalByMonthAsync(targetMonth);
    }

    public Task<IReadOnlyList<Expense>> GetRecentUnexpectedAsync(DateOnly targetMonth, int limit)
    {
        return _expenseRepository.GetRecentUnexpectedAsync(targetMonth, limit);
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        return _categoryRepository.GetByTypeAsync(
            CategoryType.Expense);
    }

    public async Task<long> RegisterAsync(Expense expense)
    {
        var expenseId = await _expenseRepository.InsertAsync(expense);

        _logger.LogInformation("Expense registered. ExpenseId: {ExpenseId}", expenseId);

        return expenseId;
    }

    public async Task<bool> UpdateAsync(Expense expense)
    {
        var result = await _expenseRepository.UpdateAsync(expense);

        if (result)
        {
            _logger.LogInformation(
                "Expense updated. ExpenseId: {ExpenseId}",
                expense.ExpenseId);
        }
        else
        {
            _logger.LogWarning(
                "Expense update failed. ExpenseId: {ExpenseId}",
                expense.ExpenseId);
        }

        return result;
    }

    public async Task<bool> DeleteAsync(long expenseId)
    {
        var result = await _expenseRepository.DeleteAsync(expenseId);

        if (result)
        {
            _logger.LogInformation(
                "Expense deleted. ExpenseId: {ExpenseId}",
                expenseId);
        }
        else
        {
            _logger.LogWarning(
                "Expense delete failed. ExpenseId: {ExpenseId}",
                expenseId);
        }

        return result;
    }
}