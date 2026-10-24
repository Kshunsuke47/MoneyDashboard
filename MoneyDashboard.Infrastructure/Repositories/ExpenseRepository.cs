using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public ExpenseRepository(
        ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Expense>> GetByMonthAsync(
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT
                expense_id AS ExpenseId,
                expense_date AS ExpenseDateValue,
                description AS Description,
                category_id AS CategoryId,
                amount AS Amount,
                note AS Note,
                is_unexpected AS IsUnexpectedValue
            FROM expenses
            WHERE expense_date >= @StartDate
              AND expense_date < @NextMonthStart
            ORDER BY expense_date DESC, expense_id DESC;
            """;

        var startDate =
            new DateOnly(targetMonth.Year, targetMonth.Month, 1);

        var nextMonthStart =
            startDate.AddMonths(1);

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<ExpenseRow>(
                sql,
                new
                {
                    StartDate =
                        DbValueConverter.ToDateString(startDate),

                    NextMonthStart =
                        DbValueConverter.ToDateString(nextMonthStart)
                });

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<Expense?> GetByIdAsync(long expenseId)
    {
        const string sql = """
            SELECT
                expense_id AS ExpenseId,
                expense_date AS ExpenseDateValue,
                description AS Description,
                category_id AS CategoryId,
                amount AS Amount,
                note AS Note,
                is_unexpected AS IsUnexpectedValue
            FROM expenses
            WHERE expense_id = @ExpenseId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection.QuerySingleOrDefaultAsync<ExpenseRow>(
                sql,
                new
                {
                    ExpenseId = expenseId
                });

        return row is null
            ? null
            : ToModel(row);
    }

    public async Task<long> GetTotalByMonthAsync(
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT COALESCE(SUM(amount), 0)
            FROM expenses
            WHERE expense_date >= @StartDate
              AND expense_date < @NextMonthStart;
            """;

        var startDate =
            new DateOnly(targetMonth.Year, targetMonth.Month, 1);

        var nextMonthStart =
            startDate.AddMonths(1);

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                StartDate =
                    DbValueConverter.ToDateString(startDate),

                NextMonthStart =
                    DbValueConverter.ToDateString(nextMonthStart)
            });
    }

    public async Task<IReadOnlyList<Expense>> GetRecentUnexpectedAsync(
        DateOnly targetMonth,
        int limit)
    {
        const string sql = """
            SELECT
                expense_id AS ExpenseId,
                expense_date AS ExpenseDateValue,
                description AS Description,
                category_id AS CategoryId,
                amount AS Amount,
                note AS Note,
                is_unexpected AS IsUnexpectedValue
            FROM expenses
            WHERE is_unexpected = 1
              AND expense_date < @NextMonthStart
            ORDER BY expense_date DESC, expense_id DESC
            LIMIT @Limit;
            """;

        var startDate =
            new DateOnly(targetMonth.Year, targetMonth.Month, 1);

        var nextMonthStart =
            startDate.AddMonths(1);

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<ExpenseRow>(
                sql,
                new
                {
                    NextMonthStart =
                        DbValueConverter.ToDateString(nextMonthStart),

                    Limit = limit
                });

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<long> InsertAsync(Expense expense)
    {
        const string sql = """
            INSERT INTO expenses
                (
                    expense_date,
                    description,
                    category_id,
                    amount,
                    note,
                    is_unexpected
                )
            VALUES
                (
                    @ExpenseDate,
                    @Description,
                    @CategoryId,
                    @Amount,
                    @Note,
                    @IsUnexpected
                )
            RETURNING expense_id;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                ExpenseDate =
                    DbValueConverter.ToDateString(
                        expense.ExpenseDate),

                expense.Description,
                expense.CategoryId,
                expense.Amount,
                expense.Note,

                IsUnexpected =
                    DbValueConverter.ToDbValue(
                        expense.IsUnexpected)
            });
    }

    public async Task<bool> UpdateAsync(Expense expense)
    {
        const string sql = """
            UPDATE expenses
            SET
                expense_date = @ExpenseDate,
                description = @Description,
                category_id = @CategoryId,
                amount = @Amount,
                note = @Note,
                is_unexpected = @IsUnexpected
            WHERE expense_id = @ExpenseId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    expense.ExpenseId,

                    ExpenseDate =
                        DbValueConverter.ToDateString(
                            expense.ExpenseDate),

                    expense.Description,
                    expense.CategoryId,
                    expense.Amount,
                    expense.Note,

                    IsUnexpected =
                        DbValueConverter.ToDbValue(
                            expense.IsUnexpected)
                });

        return affectedRows == 1;
    }

    public async Task<bool> DeleteAsync(long expenseId)
    {
        const string sql = """
            DELETE FROM expenses
            WHERE expense_id = @ExpenseId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    ExpenseId = expenseId
                });

        return affectedRows == 1;
    }

    private static Expense ToModel(ExpenseRow row)
    {
        return new Expense
        {
            ExpenseId = row.ExpenseId,

            ExpenseDate =
                DbValueConverter.ToDateOnly(
                    row.ExpenseDateValue),

            Description = row.Description,
            CategoryId = row.CategoryId,
            Amount = row.Amount,
            Note = row.Note,

            IsUnexpected =
                DbValueConverter.ToBoolean(
                    row.IsUnexpectedValue)
        };
    }

    private sealed class ExpenseRow
    {
        public long ExpenseId { get; set; }

        public string ExpenseDateValue { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public long CategoryId { get; set; }

        public long Amount { get; set; }

        public string? Note { get; set; }

        public long IsUnexpectedValue { get; set; }
    }
}