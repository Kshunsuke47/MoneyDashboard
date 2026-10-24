using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class NisaRepository : INisaRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public NisaRepository(
        ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MonthlyNisaValue?> GetByMonthAsync(
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT
                nisa_value_id AS NisaValueId,
                target_month AS TargetMonthValue,
                evaluation_amount AS EvaluationAmount
            FROM monthly_nisa_values
            WHERE target_month = @TargetMonth;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection.QuerySingleOrDefaultAsync<MonthlyNisaValueRow>(
                sql,
                new
                {
                    TargetMonth =
                        DbValueConverter.ToTargetMonthString(
                            targetMonth)
                });

        return row is null
            ? null
            : ToModel(row);
    }

    public async Task<bool> UpsertAsync(
        DateOnly targetMonth,
        long evaluationAmount)
    {
        const string sql = """
            INSERT INTO monthly_nisa_values
                (
                    target_month,
                    evaluation_amount
                )
            VALUES
                (
                    @TargetMonth,
                    @EvaluationAmount
                )
            ON CONFLICT(target_month)
            DO UPDATE SET
                evaluation_amount = excluded.evaluation_amount;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    TargetMonth =
                        DbValueConverter.ToTargetMonthString(
                            targetMonth),

                    EvaluationAmount = evaluationAmount
                });

        return affectedRows == 1;
    }

    private static MonthlyNisaValue ToModel(
        MonthlyNisaValueRow row)
    {
        return new MonthlyNisaValue
        {
            NisaValueId = row.NisaValueId,

            TargetMonth =
                DbValueConverter.ToTargetMonth(
                    row.TargetMonthValue),

            EvaluationAmount = row.EvaluationAmount
        };
    }

    private sealed class MonthlyNisaValueRow
    {
        public long NisaValueId { get; set; }

        public string TargetMonthValue { get; set; }
            = string.Empty;

        public long EvaluationAmount { get; set; }
    }
}