using Dapper;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public SubscriptionRepository(
        ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Subscription>> GetAllAsync()
    {
        const string sql = """
            SELECT
                subscription_id AS SubscriptionId,
                service_name AS ServiceName,
                category_id AS CategoryId,
                payment_amount AS PaymentAmount,
                payment_cycle AS PaymentCycleValue,
                payment_month AS PaymentMonth,
                payment_day AS PaymentDay,
                status AS StatusValue,
                note AS Note
            FROM subscriptions
            ORDER BY
                CASE status
                    WHEN 'ACTIVE' THEN 1
                    WHEN 'PAUSED' THEN 2
                    WHEN 'CANCELLED' THEN 3
                    ELSE 4
                END,
                service_name ASC,
                subscription_id ASC;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<SubscriptionRow>(sql);

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<IReadOnlyList<Subscription>> GetActiveAsync()
    {
        const string sql = """
            SELECT
                subscription_id AS SubscriptionId,
                service_name AS ServiceName,
                category_id AS CategoryId,
                payment_amount AS PaymentAmount,
                payment_cycle AS PaymentCycleValue,
                payment_month AS PaymentMonth,
                payment_day AS PaymentDay,
                status AS StatusValue,
                note AS Note
            FROM subscriptions
            WHERE status = 'ACTIVE'
            ORDER BY service_name ASC,
                     subscription_id ASC;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var rows =
            await connection.QueryAsync<SubscriptionRow>(sql);

        return rows
            .Select(ToModel)
            .ToList();
    }

    public async Task<Subscription?> GetByIdAsync(
        long subscriptionId)
    {
        const string sql = """
            SELECT
                subscription_id AS SubscriptionId,
                service_name AS ServiceName,
                category_id AS CategoryId,
                payment_amount AS PaymentAmount,
                payment_cycle AS PaymentCycleValue,
                payment_month AS PaymentMonth,
                payment_day AS PaymentDay,
                status AS StatusValue,
                note AS Note
            FROM subscriptions
            WHERE subscription_id = @SubscriptionId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var row =
            await connection.QuerySingleOrDefaultAsync<SubscriptionRow>(
                sql,
                new
                {
                    SubscriptionId = subscriptionId
                });

        return row is null
            ? null
            : ToModel(row);
    }

    public async Task<long> InsertAsync(
        Subscription subscription)
    {
        const string sql = """
            INSERT INTO subscriptions
                (
                    service_name,
                    category_id,
                    payment_amount,
                    payment_cycle,
                    payment_month,
                    payment_day,
                    status,
                    note
                )
            VALUES
                (
                    @ServiceName,
                    @CategoryId,
                    @PaymentAmount,
                    @PaymentCycle,
                    @PaymentMonth,
                    @PaymentDay,
                    @Status,
                    @Note
                )
            RETURNING subscription_id;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        return await connection.ExecuteScalarAsync<long>(
            sql,
            new
            {
                subscription.ServiceName,
                subscription.CategoryId,
                subscription.PaymentAmount,

                PaymentCycle =
                    DbValueConverter.ToDbValue(
                        subscription.PaymentCycle),

                subscription.PaymentMonth,
                subscription.PaymentDay,

                Status =
                    DbValueConverter.ToDbValue(
                        subscription.Status),

                subscription.Note
            });
    }

    public async Task<bool> UpdateAsync(
        Subscription subscription)
    {
        const string sql = """
            UPDATE subscriptions
            SET
                service_name = @ServiceName,
                category_id = @CategoryId,
                payment_amount = @PaymentAmount,
                payment_cycle = @PaymentCycle,
                payment_month = @PaymentMonth,
                payment_day = @PaymentDay,
                status = @Status,
                note = @Note
            WHERE subscription_id = @SubscriptionId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    subscription.SubscriptionId,
                    subscription.ServiceName,
                    subscription.CategoryId,
                    subscription.PaymentAmount,

                    PaymentCycle =
                        DbValueConverter.ToDbValue(
                            subscription.PaymentCycle),

                    subscription.PaymentMonth,
                    subscription.PaymentDay,

                    Status =
                        DbValueConverter.ToDbValue(
                            subscription.Status),

                    subscription.Note
                });

        return affectedRows == 1;
    }

    public async Task<bool> DeleteAsync(
        long subscriptionId)
    {
        const string sql = """
            DELETE FROM subscriptions
            WHERE subscription_id = @SubscriptionId;
            """;

        await using var connection =
            await _connectionFactory.CreateOpenConnectionAsync();

        var affectedRows =
            await connection.ExecuteAsync(
                sql,
                new
                {
                    SubscriptionId = subscriptionId
                });

        return affectedRows == 1;
    }

    private static Subscription ToModel(
        SubscriptionRow row)
    {
        return new Subscription
        {
            SubscriptionId = row.SubscriptionId,
            ServiceName = row.ServiceName,
            CategoryId = row.CategoryId,
            PaymentAmount = row.PaymentAmount,

            PaymentCycle =
                DbValueConverter.ToPaymentCycle(
                    row.PaymentCycleValue),

            PaymentMonth = row.PaymentMonth,
            PaymentDay = row.PaymentDay,

            Status =
                DbValueConverter.ToSubscriptionStatus(
                    row.StatusValue),

            Note = row.Note
        };
    }

    private sealed class SubscriptionRow
    {
        public long SubscriptionId { get; set; }

        public string ServiceName { get; set; }
            = string.Empty;

        public long CategoryId { get; set; }

        public long PaymentAmount { get; set; }

        public string PaymentCycleValue { get; set; }
            = string.Empty;

        public int? PaymentMonth { get; set; }

        public int PaymentDay { get; set; }

        public string StatusValue { get; set; }
            = string.Empty;

        public string? Note { get; set; }
    }
}