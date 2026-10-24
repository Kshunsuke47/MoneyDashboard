using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class MonthlyBankBalanceRepository
    : IMonthlyBankBalanceRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public MonthlyBankBalanceRepository(
        ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<MonthlyBankBalance>> GetByMonthAsync(
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT
                monthly_balance_id AS MonthlyBalanceId,
                account_id AS AccountId,
                target_month AS TargetMonthValue,
                opening_balance AS OpeningBalance
            FROM monthly_bank_balances
            WHERE target_month = @TargetMonth
            ORDER BY account_id ASC;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<MonthlyBankBalanceRow>(
                sql,
                new
                {
                    TargetMonth =
                        DbValueConverter.ToTargetMonthString(
                            targetMonth)
                });

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<MonthlyBankBalance?> GetByAccountAndMonthAsync(
        long accountId,
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT
                monthly_balance_id AS MonthlyBalanceId,
                account_id AS AccountId,
                target_month AS TargetMonthValue,
                opening_balance AS OpeningBalance
            FROM monthly_bank_balances
            WHERE account_id = @AccountId
              AND target_month = @TargetMonth;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection
                .QuerySingleOrDefaultAsync<MonthlyBankBalanceRow>(
                    sql,
                    new
                    {
                        AccountId = accountId,

                        TargetMonth =
                            DbValueConverter.ToTargetMonthString(
                                targetMonth)
                    });

        return row is null
            ? null
            : ToModel(row);
    }

    public async Task<long> GetTotalByMonthAsync(
        DateOnly targetMonth)
    {
        const string sql = """
            SELECT COALESCE(SUM(opening_balance), 0)
            FROM monthly_bank_balances
            WHERE target_month = @TargetMonth;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                TargetMonth =
                    DbValueConverter.ToTargetMonthString(
                        targetMonth)
            });
    }

    public async Task<bool> UpsertAsync(
        long accountId,
        DateOnly targetMonth,
        long openingBalance)
    {
        const string sql = """
            INSERT INTO monthly_bank_balances
                (
                    account_id,
                    target_month,
                    opening_balance
                )
            VALUES
                (
                    @AccountId,
                    @TargetMonth,
                    @OpeningBalance
                )
            ON CONFLICT(account_id, target_month)
            DO UPDATE SET
                opening_balance = excluded.opening_balance;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    AccountId = accountId,

                    TargetMonth =
                        DbValueConverter.ToTargetMonthString(
                            targetMonth),

                    OpeningBalance = openingBalance
                });

        return affectedRows == 1;
    }

    private static MonthlyBankBalance ToModel(
        MonthlyBankBalanceRow row)
    {
        return new MonthlyBankBalance
        {
            MonthlyBalanceId = row.MonthlyBalanceId,
            AccountId = row.AccountId,

            TargetMonth =
                DbValueConverter.ToTargetMonth(
                    row.TargetMonthValue),

            OpeningBalance = row.OpeningBalance
        };
    }

    private sealed class MonthlyBankBalanceRow
    {
        public long MonthlyBalanceId { get; set; }

        public long AccountId { get; set; }

        public string TargetMonthValue { get; set; }
            = string.Empty;

        public long OpeningBalance { get; set; }
    }
}