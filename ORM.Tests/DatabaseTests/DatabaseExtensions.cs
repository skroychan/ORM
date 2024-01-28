using skroy.ORM.Adapters;
using System.Reflection;

namespace skroy.ORM.Tests.DatabaseTests;

public static class DatabaseExtensions
{
	public static void DropTable(this Database database, string table)
	{
		GetAdapter(database).ExecuteNonQuery($"drop table if exists [{table}]");
	}

	public static void Truncate(this Database database, string table)
	{
		GetAdapter(database).ExecuteNonQuery($"delete from [{table}]");
	}


	private static IDbAdapter GetAdapter(Database database)
	{
		var field = typeof(Database).GetField("adapter", BindingFlags.Instance | BindingFlags.NonPublic);
		return (IDbAdapter)field.GetValue(database);
	}
}
