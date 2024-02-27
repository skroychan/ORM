using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using skroy.ORM.Adapters;
using skroy.ORM.Dialects;
using skroy.ORM.Mapper;
using System.Linq.Expressions;

namespace skroy.ORM;

public class Database
{
	private readonly ISqlMapper mapper;
	private readonly IDbAdapter adapter;


	private Database(IDbAdapter dbAdapter, ISqlMapper sqlMapper)
    {
		mapper = sqlMapper;
		adapter = dbAdapter;
	}


	public static Database GetSqliteDatabase(string connectionString)
	{
		var mapper = new SqlMapper(new SqliteDialect());
		var adapter = new DbAdapter<SqliteConnection>(connectionString);
		var database = new Database(adapter, mapper);

		return database;
	}

	public static Database GetMySqlDatabase(string connectionString)
	{
		var mapper = new SqlMapper(new MySqlDialect());
		var adapter = new DbAdapter<MySqlConnection>(connectionString);
		var database = new Database(adapter, mapper);

		return database;
	}


	public Mapping<T>.Builder GetMappingBuilder<T>() where T : class
	{
		return new Mapping<T>.Builder();
	}

    public void AddMapping<T>(Mapping<T>.Builder mappingBuilder = null) where T : class
	{
		mappingBuilder ??= GetMappingBuilder<T>();
		mapper.AddMapping(mappingBuilder);
	}

	public long Initialize()
	{
		var query = mapper.MapCreate();
		return adapter.ExecuteNonQuery(query);
	}

	public object Insert<T>(T obj) where T : class
	{
		var query = mapper.MapInsert(obj);
		return adapter.ExecuteScalar<object>(query);
	}

	public object Insert<T>(params T[] objs) where T : class
	{
		var query = mapper.MapInsert(objs);
		return adapter.ExecuteScalar<object>(query);
	}

	public IEnumerable<T> Select<T>() where T : class, new()
	{
		var query = mapper.MapSelect<T>();
		return adapter.ExecuteVector<T>(query, [.. mapper.GetColumns<T>()]);
	}

	public T Select<T>(T obj) where T : class, new()
	{
		var query = mapper.MapSelect(obj);
		return adapter.ExecuteVector<T>(query, [.. mapper.GetColumns<T>()]).SingleOrDefault();
	}

	public long Update<T>(T obj) where T : class
	{
		var query = mapper.MapUpdate(obj);
		return adapter.ExecuteNonQuery(query);
	}

	public long Update<T>(T obj, Expression<Action<T>> memberInitializer) where T : class
	{
		var query = mapper.MapUpdate(obj, memberInitializer);
		return adapter.ExecuteNonQuery(query);
	}

	public long Delete<T>(T obj) where T : class
	{
		var query = mapper.MapDelete(obj);
		return adapter.ExecuteNonQuery(query);
	}

	public long Delete<T>(Expression<Func<T, bool>> predicate) where T : class
	{
		var query = mapper.MapDelete(predicate);
		return adapter.ExecuteNonQuery(query);
	}
}
