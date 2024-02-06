using skroy.ORM.Mapper;

namespace skroy.ORM.Adapters;

internal class MySqlAdapter : IDbAdapter
{
	public long ExecuteNonQuery(string query)
	{
		throw new NotImplementedException();
	}

	public T ExecuteScalar<T>(string query)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<T> ExecuteVector<T>(string query, params Column[] columns) where T : new()
	{
		throw new NotImplementedException();
	}
}
