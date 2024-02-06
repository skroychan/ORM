using skroy.ORM.Mapper;

namespace skroy.ORM.Adapters;

internal interface IDbAdapter
{
	public long ExecuteNonQuery(string query);
	public T ExecuteScalar<T>(string query);
	public IEnumerable<T> ExecuteVector<T>(string query, params Column[] columns) where T : new();
}
