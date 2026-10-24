using Microsoft.Data.Sqlite;

namespace MoneyDashboard.Infrastructure.Database;

public interface ISqliteConnectionFactory
{
    Task<SqliteConnection> CreateOpenConnectionAsync();
}
