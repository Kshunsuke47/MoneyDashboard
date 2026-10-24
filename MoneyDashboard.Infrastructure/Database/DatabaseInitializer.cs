using Dapper;
using Microsoft.Extensions.Logging;

namespace MoneyDashboard.Infrastructure.Database;

public class DatabaseInitializer
{
    private const int CurrentVersion = 1;

    private readonly ISqliteConnectionFactory _connectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(ISqliteConnectionFactory connectionFactory, ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();

        const string checkSchemaInfoSql = """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = 'schema_info';
            """;

        var schemaInfoExists = await connection.ExecuteScalarAsync<long>(checkSchemaInfoSql) > 0;

        if (schemaInfoExists)
        {
            var version = await connection.ExecuteScalarAsync<long?>("SELECT MAX(version) FROM schema_info;");

            if (version != CurrentVersion)
            {
                throw new InvalidOperationException($"Unsupported database version. Version: {version}");
            }

            return;
        }

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string createTablesSql = """
                CREATE TABLE IF NOT EXISTS schema_info (
                    version INTEGER PRIMARY KEY,
                    updated_at TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS categories (
                    category_id INTEGER PRIMARY KEY,
                    category_name TEXT NOT NULL
                        CHECK(length(category_name) BETWEEN 1 AND 50),
                    category_type TEXT NOT NULL
                        CHECK(category_type IN (
                            'INCOME',
                            'EXPENSE',
                            'SUBSCRIPTION'
                        )),
                    UNIQUE(category_name, category_type)
                );

                CREATE TABLE IF NOT EXISTS incomes (
                    income_id INTEGER PRIMARY KEY,
                    income_date TEXT NOT NULL,
                    description TEXT NOT NULL
                        CHECK(length(description) BETWEEN 1 AND 50),
                    category_id INTEGER NOT NULL,
                    amount INTEGER NOT NULL
                        CHECK(amount BETWEEN 1 AND 999999999),
                    note TEXT
                        CHECK(note IS NULL OR length(note) <= 200),

                    FOREIGN KEY(category_id)
                        REFERENCES categories(category_id)
                        ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS expenses (
                    expense_id INTEGER PRIMARY KEY,
                    expense_date TEXT NOT NULL,
                    description TEXT NOT NULL
                        CHECK(length(description) BETWEEN 1 AND 50),
                    category_id INTEGER NOT NULL,
                    amount INTEGER NOT NULL
                        CHECK(amount BETWEEN 1 AND 999999999),
                    note TEXT
                        CHECK(note IS NULL OR length(note) <= 200),
                    is_unexpected INTEGER NOT NULL DEFAULT 0
                        CHECK(is_unexpected IN (0, 1)),

                    FOREIGN KEY(category_id)
                        REFERENCES categories(category_id)
                        ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS subscriptions (
                    subscription_id INTEGER PRIMARY KEY,
                    service_name TEXT NOT NULL
                        CHECK(length(service_name) BETWEEN 1 AND 50),
                    category_id INTEGER NOT NULL,
                    payment_amount INTEGER NOT NULL
                        CHECK(payment_amount BETWEEN 1 AND 999999999),
                    payment_cycle TEXT NOT NULL DEFAULT 'MONTHLY'
                        CHECK(payment_cycle IN ('MONTHLY', 'YEARLY')),
                    payment_month INTEGER,
                    payment_day INTEGER NOT NULL
                        CHECK(payment_day BETWEEN 1 AND 31),
                    status TEXT NOT NULL DEFAULT 'ACTIVE'
                        CHECK(status IN (
                            'ACTIVE',
                            'PAUSED',
                            'CANCELLED'
                        )),
                    note TEXT
                        CHECK(note IS NULL OR length(note) <= 200),

                    CHECK(
                        (payment_cycle = 'MONTHLY'
                            AND payment_month IS NULL)
                        OR
                        (payment_cycle = 'YEARLY'
                            AND payment_month BETWEEN 1 AND 12)
                    ),

                    FOREIGN KEY(category_id)
                        REFERENCES categories(category_id)
                        ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS bank_accounts (
                    account_id INTEGER PRIMARY KEY,
                    account_name TEXT NOT NULL
                        CHECK(length(account_name) BETWEEN 1 AND 50),
                    is_active INTEGER NOT NULL DEFAULT 1
                        CHECK(is_active IN (0, 1))
                );

                CREATE TABLE IF NOT EXISTS monthly_bank_balances (
                    monthly_balance_id INTEGER PRIMARY KEY,
                    account_id INTEGER NOT NULL,
                    target_month TEXT NOT NULL,
                    opening_balance INTEGER NOT NULL
                        CHECK(opening_balance BETWEEN 0 AND 999999999),

                    UNIQUE(account_id, target_month),

                    FOREIGN KEY(account_id)
                        REFERENCES bank_accounts(account_id)
                        ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS monthly_nisa_values (
                    nisa_value_id INTEGER PRIMARY KEY,
                    target_month TEXT NOT NULL UNIQUE,
                    evaluation_amount INTEGER NOT NULL
                        CHECK(evaluation_amount BETWEEN 0 AND 999999999)
                );
                """;

            await connection.ExecuteAsync(
                createTablesSql,
                transaction: transaction);

            const string insertCategoriesSql = """
                INSERT INTO categories
                    (category_name, category_type)
                VALUES
                    ('給与', 'INCOME'),
                    ('賞与', 'INCOME'),
                    ('副業', 'INCOME'),
                    ('その他', 'INCOME'),

                    ('食費', 'EXPENSE'),
                    ('住居費', 'EXPENSE'),
                    ('水道光熱費', 'EXPENSE'),
                    ('通信費', 'EXPENSE'),
                    ('日用品', 'EXPENSE'),
                    ('交通費', 'EXPENSE'),
                    ('医療費', 'EXPENSE'),
                    ('娯楽費', 'EXPENSE'),
                    ('交際費', 'EXPENSE'),
                    ('その他', 'EXPENSE'),

                    ('動画', 'SUBSCRIPTION'),
                    ('音楽', 'SUBSCRIPTION'),
                    ('ソフトウェア', 'SUBSCRIPTION'),
                    ('クラウド', 'SUBSCRIPTION'),
                    ('ショッピング', 'SUBSCRIPTION'),
                    ('その他', 'SUBSCRIPTION');
                """;

            await connection.ExecuteAsync(
                insertCategoriesSql,
                transaction: transaction);

            const string insertVersionSql = """
                INSERT INTO schema_info
                    (version, updated_at)
                VALUES
                    (@Version, @UpdatedAt);
                """;

            await connection.ExecuteAsync(
                insertVersionSql,
                new
                {
                    Version = CurrentVersion,
                    UpdatedAt = DateTimeOffset.Now.ToString("O")
                },
                transaction);

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Database initialized. Version: {Version}",
                CurrentVersion);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}