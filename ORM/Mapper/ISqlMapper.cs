using System.Linq.Expressions;

namespace skroy.ORM.Mapper;

internal interface ISqlMapper
{
	public void AddMapping<T>(Mapping<T>.Builder mappingBuilder) where T : class;
	public IEnumerable<Column> GetColumns<T>();
	public string MapCreate();
	public string MapInsert<T>(T obj) where T : class;
	public string MapInsert<T>(params T[] obj) where T : class;
	public string MapSelect<T>() where T : class;
	public string MapSelect<T>(T obj) where T : class;
	public string MapUpdate<T>(T obj) where T : class;
	public string MapUpdate<T>(T obj, Expression<Action<T>> memberInitializer) where T : class;
	public string MapDelete<T>(T obj) where T : class;
	public string MapDelete<T>(Expression<Func<T, bool>> predicate) where T : class;
}
