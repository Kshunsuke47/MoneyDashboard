using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public BankAccountRepository(
        ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<BankAccount>> GetAllAsync()
    {
        const string sql = """
            SELECT
                account_id AS AccountId,
                account_name AS AccountName,
                is_active AS IsActiveValue
            FROM bank_accounts
            ORDER BY account_id ASC;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<BankAccountRow>(sql);

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<BankAccount?> GetByIdAsync(long accountId)
    {
        const string sql = """
            SELECT
                account_id AS AccountId,
                account_name AS AccountName,
                is_active AS IsActiveValue
            FROM bank_accounts
            WHERE account_id = @AccountId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection.QuerySingleOrDefaultAsync<BankAccountRow>(
                sql,
                new
                {
                    AccountId = accountId
                });

        return row is null
            ? null
            : ToModel(row);
    }

    public async Task<long> InsertAsync(
        BankAccount bankAccount)
    {
        const string sql = """
            INSERT INTO bank_accounts
                (
                    account_name,
                    is_active
                )
            VALUES
                (
                    @AccountName,
                    1
                )
            RETURNING account_id;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                bankAccount.AccountName
            });
    }

    public async Task<bool> UpdateAsync(
        long accountId,
        string accountName)
    {
        const string sql = """
            UPDATE bank_accounts
            SET
                account_name = @AccountName
            WHERE account_id = @AccountId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    AccountId = accountId,
                    AccountName = accountName
                });

        return affectedRows == 1;
    }

    public async Task<bool> DeactivateAsync(long accountId)
    {
        const string sql = """
            UPDATE bank_accounts
            SET
                is_active = 0
            WHERE account_id = @AccountId
              AND is_active = 1;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    AccountId = accountId
                });

        return affectedRows == 1;
    }

    private static BankAccount ToModel(
        BankAccountRow row)
    {
        return new BankAccount
        {
            AccountId = row.AccountId,
            AccountName = row.AccountName,

            IsActive =
                DbValueConverter.ToBoolean(
                    row.IsActiveValue)
        };
    }

    private sealed class BankAccountRow
    {
        public long AccountId { get; set; }

        public string AccountName { get; set; }
            = string.Empty;

        public long IsActiveValue { get; set; }
    }
}