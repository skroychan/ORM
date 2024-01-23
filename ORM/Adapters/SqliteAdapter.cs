using Microsoft.Data.Sqlite;
using skroy.ORM.Mapper;

namespace skroy.ORM.Adapters;

public class SqliteAdapter : IDbAdapter
{
	private readonly SqliteConnection connection;


    public SqliteAdapter(string connectionString)
    {
		try
		{
			connection = new SqliteConnection(connectionString);
			connection.Open();
		}
		catch
		{
			connection?.Close();
			throw;
		}
    }


	public int ExecuteNonQuery(string query)
	{
		using var transaction = connection.BeginTransaction();

		try
		{
			using var command = new SqliteCommand(query, connection, transaction);
			var result = command.ExecuteNonQuery();

			transaction.Commit();
			return result;
		}
		catch
		{
			transaction.Rollback();
			throw;
		}
	}

	public T ExecuteScalar<T>(string query)
	{
		using var transaction = connection.BeginTransaction();

		try
		{
			using var command = new SqliteCommand(query, connection, transaction);
			var result = (T)command.ExecuteScalar();

			transaction.Commit();
			return result;
		}
		catch
		{
			transaction.Rollback();
			throw;
		}
	}

	public List<T> ExecuteVector<T>(string query, params Column[] columns) where T : new()
	{
		var result = new List<T>();
		using var transaction = connection.BeginTransaction();

		try
		{
			using var command = new SqliteCommand(query, connection, transaction);
			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var obj = new T();
				foreach (var column in columns)
				{
					var value = GetValue(reader, column);
					typeof(T).GetProperty(column.Name).SetValue(obj, value);
				}
				result.Add(obj);
			}

			transaction.Commit();
			return result;
		}
		catch
		{
			transaction.Rollback();
			throw;
		}

		object GetValue(SqliteDataReader reader, Column column)
		{
			if (reader.IsDBNull(reader.GetOrdinal(column.Name)))
				return null;

			if (column.Type.IsEnum && Enum.TryParse(column.Type, reader[column.Name].ToString(), out var result))
				return result;
			
			return Convert.ChangeType(reader[column.Name], column.Type);
		}
	}
}
