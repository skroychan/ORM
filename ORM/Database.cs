using skroy.ORM.Adapters;
using skroy.ORM.Mapper;
using System.Linq.Expressions;

namespace skroy.ORM;

public class Database
{
	private readonly SqlMapper sqlMapper;
	private readonly IDbAdapter adapter;


	public Database(DatabaseProvider databaseProvider, string connectionString)
    {
		var (sqlDialect, dbAdapter) = Configuration.GetConfiguration(databaseProvider, connectionString);
		sqlMapper = new SqlMapper(sqlDialect);
		adapter = dbAdapter;
	}


	public Mapping<T>.MappingBuilder GetMappingBuilder<T>() where T : class
	{
		return new Mapping<T>.MappingBuilder();
	}

    public void AddMapping<T>(Mapping<T>.MappingBuilder mappingBuilder = null) where T : class
	{
		mappingBuilder ??= GetMappingBuilder<T>();
		sqlMapper.AddMapping(mappingBuilder);
	}

	public long Initialize()
	{
		var query = sqlMapper.MapCreate();
		return adapter.ExecuteNonQuery(query);
	}

	public object Insert<T>(T obj) where T : class
	{
		var query = sqlMapper.MapInsert(obj);
		return adapter.ExecuteScalar<object>(query);
	}

	public object Insert<T>(params T[] objs) where T : class
	{
		var query = sqlMapper.MapInsert(objs);
		return adapter.ExecuteScalar<object>(query);
	}

	public IEnumerable<T> Select<T>() where T : class, new()
	{
		var query = sqlMapper.MapSelect<T>();
		return adapter.ExecuteVector<T>(query, [.. sqlMapper.GetColumns<T>()]);
	}

	public T Select<T>(T obj) where T : class, new()
	{
		var query = sqlMapper.MapSelect(obj);
		return adapter.ExecuteVector<T>(query, [.. sqlMapper.GetColumns<T>()]).SingleOrDefault();
	}

	public long Update<T>(T obj) where T : class
	{
		var query = sqlMapper.MapUpdate(obj);
		return adapter.ExecuteNonQuery(query);
	}

	public long Update<T>(T obj, Expression<Action<T>> memberInitializer) where T : class
	{
		var query = sqlMapper.MapUpdate(obj, memberInitializer);
		return adapter.ExecuteNonQuery(query);
	}

	public long Delete<T>(T obj) where T : class
	{
		var query = sqlMapper.MapDelete(obj);
		return adapter.ExecuteNonQuery(query);
	}

	public long Delete<T>(Expression<Func<T, bool>> predicate) where T : class
	{
		var query = sqlMapper.MapDelete(predicate);
		return adapter.ExecuteNonQuery(query);
	}
}
