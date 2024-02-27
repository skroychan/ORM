using skroy.ORM.Mapper;
using System.Data.Common;

namespace skroy.ORM.Adapters;

internal class DbAdapter<TConnection> : IDbAdapter where TConnection : DbConnection, new()
{
	private readonly TConnection connection;


    private DbAdapter() 
	{
	}

	public DbAdapter(string connectionString)
	{
		try
		{
			connection = new TConnection();
			connection.ConnectionString = connectionString;
			connection.Open();
		}
		catch
		{
			connection?.Close();
			throw;
		}
	}


	public long ExecuteNonQuery(string query)
	{
		using var transaction = connection.BeginTransaction();
		try
		{
			using var command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = query;

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
			using var command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = query;

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

	public IEnumerable<T> ExecuteVector<T>(string query, params Column[] columns) where T : new()
	{
		var result = new List<T>();
		using var transaction = connection.BeginTransaction();

		try
		{
			using var command = connection.CreateCommand();
			command.Transaction = transaction;
			command.CommandText = query;

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

		object GetValue(DbDataReader reader, Column column)
		{
			if (reader.IsDBNull(reader.GetOrdinal(column.Name)))
				return null;

			if (column.Type.IsEnum && Enum.TryParse(column.Type, reader[column.Name].ToString(), out var result))
				return result;

			return Convert.ChangeType(reader[column.Name], column.Type);
		}
	}
}
