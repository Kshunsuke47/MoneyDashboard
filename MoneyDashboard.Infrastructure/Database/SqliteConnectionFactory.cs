using Microsoft.Data.Sqlite;

namespace MoneyDashboard.Infrastructure.Database;

public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory()
    {
        var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MoneyDashboard");

        Directory.CreateDirectory(folderPath);

        var databasePath = Path.Combine(folderPath, "moneydashboard.db");

        _connectionString = $"Data Source={databasePath}";
    }

    public async Task<SqliteConnection> CreateOpenConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = "PRAGMA foreign_keys = ON;";

        await command.ExecuteNonQueryAsync();

        return connection;
    }
}