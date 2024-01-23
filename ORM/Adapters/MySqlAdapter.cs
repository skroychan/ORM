using skroy.ORM.Mapper;

namespace skroy.ORM.Adapters;

public class MySqlAdapter : IDbAdapter
{
	public long ExecuteNonQuery(string query)
	{
		throw new NotImplementedException();
	}

	public T ExecuteScalar<T>(string query)
	{
		throw new NotImplementedException();
	}

	public List<T> ExecuteVector<T>(string query, params Column[] columns) where T : new()
	{
		throw new NotImplementedException();
	}
}
