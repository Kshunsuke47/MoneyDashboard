using Dapper;
using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Repositories;
using MoneyDashboard.Infrastructure.Database;

namespace MoneyDashboard.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ISqliteConnectionFactory _connectionFactory;

    public CategoryRepository(ISqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Category>> GetByTypeAsync(CategoryType categoryType)
    {
        const string sql = """
            SELECT
                category_id AS CategoryId,
                category_name AS CategoryName,
                category_type AS CategoryTypeValue
            FROM categories
            WHERE category_type = @CategoryType
            ORDER BY category_id ASC;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync();

        var rows = await connection.QueryAsync<CategoryRow>(
            sql,
            new
            {
                CategoryType = DbValueConverter.ToDbValue(categoryType)
            });

        return rows
            .Select(row => new Category
            {
                CategoryId = row.CategoryId,
                CategoryName = row.CategoryName,
                CategoryType = DbValueConverter.ToCategoryType(row.CategoryTypeValue)
            })
            .ToList();
    }

    private sealed class CategoryRow
    {
        public long CategoryId { get; set; }

        public string CategoryName { get; set; }
            = string.Empty;

        public string CategoryTypeValue { get; set; }
            = string.Empty;
    }
}