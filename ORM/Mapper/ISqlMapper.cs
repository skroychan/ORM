using System.Linq.Expressions;

namespace skroy.ORM.Mapper;

public interface ISqlMapper
{
	public void AddMapping<T>(Mapping<T> mapping) where T : class;
	public List<Column> GetColumns(Type mapping);
	public string MapCreate();
	public string MapInsert<T>(T obj) where T : class;
	public string MapInsert<T>(params T[] obj) where T : class;
	public string MapSelect<T>() where T : class;
	public string MapSelect<T>(T obj) where T : class;
	public string MapUpdate<T>(T obj) where T : class;
	public string MapUpdate<T>(T obj, Expression<Action<T>> memberInitializer) where T : class;
	public string MapDelete<T>(T obj) where T : class;
	public string MapDelete<T>(Expression<Func<T, bool>> predicate) where T : class;
	public string MapDrop<T>() where T : class;
}
