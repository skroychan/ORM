using skroy.ORM.Mapper;

namespace skroy.ORM.Adapters;

public interface IDbAdapter
{
	public int ExecuteNonQuery(string query);
	public T ExecuteScalar<T>(string query);
	public List<T> ExecuteVector<T>(string query, params Column[] columns) where T : new();
}
