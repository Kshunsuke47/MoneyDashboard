using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class IncomeRepository : IIncomeRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public IncomeRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Income>> GetByMonthAsync(DateOnly targetMonth)
    {
        const string sql = """
            SELECT
                income_id AS IncomeId,
                income_date AS IncomeDateValue,
                description AS Description,
                category_id AS CategoryId,
                amount AS Amount,
                note AS Note
            FROM incomes
            WHERE income_date >= @StartDate
              AND income_date < @NextMonthStart
            ORDER BY income_date DESC, income_id DESC;
            """;

        var startDate =
            new DateOnly(targetMonth.Year, targetMonth.Month, 1);

        var nextMonthStart =
            startDate.AddMonths(1);

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<IncomeRow>(
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

    public async Task<Income?> GetByIdAsync(long incomeId)
    {
        const string sql = """
            SELECT
                income_id AS IncomeId,
                income_date AS IncomeDateValue,
                description AS Description,
                category_id AS CategoryId,
                amount AS Amount,
                note AS Note
            FROM incomes
            WHERE income_id = @IncomeId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection.QuerySingleOrDefaultAsync<IncomeRow>(
                sql,
                new
                {
                    IncomeId = incomeId
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
            FROM incomes
            WHERE income_date >= @StartDate
              AND income_date < @NextMonthStart;
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

    public async Task<long> InsertAsync(Income income)
    {
        const string sql = """
            INSERT INTO incomes
                (
                    income_date,
                    description,
                    category_id,
                    amount,
                    note
                )
            VALUES
                (
                    @IncomeDate,
                    @Description,
                    @CategoryId,
                    @Amount,
                    @Note
                )
            RETURNING income_id;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                IncomeDate =
                    DbValueConverter.ToDateString(
                        income.IncomeDate),

                income.Description,
                income.CategoryId,
                income.Amount,
                income.Note
            });
    }

    public async Task<bool> UpdateAsync(Income income)
    {
        const string sql = """
            UPDATE incomes
            SET
                income_date = @IncomeDate,
                description = @Description,
                category_id = @CategoryId,
                amount = @Amount,
                note = @Note
            WHERE income_id = @IncomeId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    income.IncomeId,

                    IncomeDate =
                        DbValueConverter.ToDateString(
                            income.IncomeDate),

                    income.Description,
                    income.CategoryId,
                    income.Amount,
                    income.Note
                });

        return affectedRows == 1;
    }

    public async Task<bool> DeleteAsync(long incomeId)
    {
        const string sql = """
            DELETE FROM incomes
            WHERE income_id = @IncomeId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    IncomeId = incomeId
                });

        return affectedRows == 1;
    }

    private static Income ToModel(IncomeRow row)
    {
        return new Income
        {
            IncomeId = row.IncomeId,

            IncomeDate =
                DbValueConverter.ToDateOnly(
                    row.IncomeDateValue),

            Description = row.Description,
            CategoryId = row.CategoryId,
            Amount = row.Amount,
            Note = row.Note
        };
    }

    private sealed class IncomeRow
    {
        public long IncomeId { get; set; }

        public string IncomeDateValue { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public long CategoryId { get; set; }

        public long Amount { get; set; }

        public string? Note { get; set; }
    }
}