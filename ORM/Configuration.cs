using skroy.ORM.Adapters;
using skroy.ORM.Dialects;

namespace skroy.ORM;

public static class Configuration
{
    public static (Dialect, IDbAdapter) GetConfiguration(DatabaseProvider databaseProvider, string connectionString)
    {
        return databaseProvider switch
		{
			DatabaseProvider.Sqlite => (new SqliteDialect(), new SqliteAdapter(connectionString)),
			DatabaseProvider.MySQL => throw new NotImplementedException(),
			_ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), databaseProvider, null)
		};
    }
}
