using skroy.ORM.Dialects;
using System.Linq.Expressions;
using System.Text;

namespace skroy.ORM.Mapper;

internal class SqlMapper : ISqlMapper
{
    private readonly Dialect dialect;
    private readonly Dictionary<Type, Mapping> Mappings;


    public SqlMapper(Dialect sqlDialect)
    {
        dialect = sqlDialect;
        Mappings = [];
    }


    public void AddMapping<T>(Mapping<T>.MappingBuilder mappingBuilder) where T : class
    {
		var mapping = mappingBuilder.Build();
		foreach (var foreignKey in mapping.Columns.Where(x => x.Name.EndsWith(Mapping<T>.defaultPrimaryKey)))
		{
            var referencedTypeName = foreignKey.Name[..foreignKey.Name.LastIndexOf(Mapping<T>.defaultPrimaryKey)];
            var referencedType = Mappings.Keys.SingleOrDefault(x => x.Name == referencedTypeName);
            if (referencedType != null)
            {
                var param = Expression.Parameter(typeof(T));
                var body = Expression.Property(param, foreignKey.Name);
                var convert = Expression.Convert(body, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(convert, param);
                mappingBuilder.AddForeignKey(lambda, referencedType);
            }
		}
		Mappings[typeof(T)] = mappingBuilder.Build();
    }

    public IEnumerable<Column> GetColumns<T>()
    {
        return Mappings[typeof(T)].Columns;
    }

    public string MapCreate()
    {
        var sb = new StringBuilder();
        foreach (var mapping in Mappings.Values)
        {
            sb.Append($"create table if not exists [{mapping.TableName}] (");

            foreach (var column in mapping.Columns)
            {
                sb.Append('[').Append(column.Name).Append("] ")
                    .Append(dialect.GetDbType(column.Type));
                if (column == mapping.PrimaryKey)
                    sb.Append(" primary key ")
                        .Append(dialect.AutoIncrement)
                        .Append(" not null unique");
                else if (!column.IsNullable)
                    sb.Append(" not null");
                sb.Append(',');
            }

            foreach (var foreignKey in mapping.ForeignKeys)
            {
                if (!Mappings.TryGetValue(foreignKey.Value, out var referencedTable))
                    throw new ArgumentException($"Mapping of referenced table of type {foreignKey.Value} doesn't exist.");

                sb.Append("foreign key([")
                    .Append(foreignKey.Key.Name)
                    .Append("]) references [")
                    .Append(referencedTable.TableName)
                    .Append("]([").Append(referencedTable.PrimaryKey.Name).Append("])")
                    .Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(");");

            foreach (var index in mapping.Indices)
            {
                sb.Append("create ");
                if (index.IsUnique)
                    sb.Append("unique ");
				var indexColumns = index.Columns.Select(x => x.Name).ToArray();
                sb.Append("index ")
                    .Append("idx_")
                    .Append(string.Join("_", indexColumns))
                    .Append(" on [")
                    .Append(mapping.TableName)
                    .Append("] (")
                    .Append(string.Join(',', indexColumns.Select(x => $"[{x}]")))
                    .Append(");");
            }
        }

        return sb.ToString();
    }

	public string MapInsert<T>(T obj) where T : class
	{
		var mapping = GetMapping<T>();
		var columns = mapping.Columns.Where(x => x.Name != mapping.PrimaryKey.Name);
		var columnNames = string.Join(',', columns.Select(x => $"[{x.Name}]"));
        var values = $"({string.Join(',', columns.Select(column => GetPropertyStringValue(obj, column.Name)))})";
		return $"insert into [{mapping.TableName}] ({columnNames}) values {string.Join(',', values)}; {dialect.SelectLastRow};";
	}

	public string MapInsert<T>(params T[] objs) where T : class
    {
        if (objs.Length == 0 || objs.Length > 1000)
            throw new ArgumentException("Insert statement must have between 1 and 1000 values.");

        var mapping = GetMapping<T>();
		var columns = mapping.Columns.Where(x => x.Name != mapping.PrimaryKey.Name);
		var columnNames = string.Join(',', columns.Select(x => $"[{x.Name}]"));
        var values = objs.Select(obj => $"({string.Join(',', columns.Select(column => GetPropertyStringValue(obj, column.Name)))})").ToList();
		return $"insert into [{mapping.TableName}] ({columnNames}) values {string.Join(',', values)}; {dialect.SelectLastRow};";
    }

    public string MapSelect<T>() where T : class
    {
        var mapping = GetMapping<T>();
        return $"select * from [{mapping.TableName}];";
    }

    public string MapSelect<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"select * from [{mapping.TableName}] where {MapPrimaryKeyCondition(obj, mapping)};";
	}

    public string MapUpdate<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"update [{mapping.TableName}] set {MapSet(mapping, obj)} where {MapPrimaryKeyCondition(obj, mapping)};";
    }

    public string MapUpdate<T>(T obj, Expression<Action<T>> memberInitializer) where T : class
    {
        var mapping = GetMapping<T>();
        return $"update [{mapping.TableName}] set {MapSet(mapping, memberInitializer)} where {MapPrimaryKeyCondition(obj, mapping)};";
    }

    public string MapDelete<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"delete from [{mapping.TableName}] where {MapPrimaryKeyCondition(obj, mapping)};";
	}

	public string MapDelete<T>(Expression<Func<T, bool>> predicate) where T : class
	{
		var mapping = GetMapping<T>();
		var body = predicate.Body as BinaryExpression;
        if (body == null || body.NodeType != ExpressionType.Equal)
            throw new ArgumentException("Expression is not a value equality comparison.");

		var isLeftParameterExpression = body.Left is MemberExpression me && me.Expression is ParameterExpression pe && pe.Name == predicate.Parameters.Single().Name;
		var (memberExpression, valueExpression) = isLeftParameterExpression ? (body.Left, body.Right) : (body.Right, body.Left);
		var member = ((MemberExpression)memberExpression).Member.Name;
		var value = Expression.Lambda(valueExpression).Compile().DynamicInvoke();
		return $"delete from [{mapping.TableName}] where [{member}]={GetStringValue(value)};";
	}


	private Mapping<T> GetMapping<T>() where T : class
    {
        if (!Mappings.TryGetValue(typeof(T), out var mapping))
            throw new ArgumentException($"Cannot find mapping for {typeof(T)}.");

        return (Mapping<T>)mapping;
    }

    private static string MapPrimaryKeyCondition<T>(T obj, Mapping<T> mapping) where T : class
    {
        return MapAssignment(obj, mapping.PrimaryKey.Name);
    }

    private static string MapSet<T>(Mapping<T> mapping, T obj) where T : class
    {
        const string separator = ",";
        var assignments = new StringBuilder();
		var columns = mapping.Columns.Where(x => x.Name != mapping.PrimaryKey.Name);
		foreach (var column in columns)
            assignments.Append(MapAssignment(obj, column.Name)).Append(separator);
        assignments.Length -= separator.Length;
        return assignments.ToString();
    }

    private static string MapSet<T>(Mapping<T> mapping, Expression<Action<T>> memberInitializer) where T : class
    {
        const string separator = ",";
        var assignments = new StringBuilder();
		if (memberInitializer.Body is not MemberInitExpression expression)
			throw new ArgumentException("Expression is not a member initializer.");

		foreach (var binding in expression.Bindings)
        {
            var column = binding.Member.Name;
            if (!mapping.Columns.Any(x => x.Name == column))
                continue;

            var assignment = ((MemberAssignment)binding).Expression;
            var value = Expression.Lambda(assignment).Compile().DynamicInvoke();
            assignments.Append($"[{column}]={GetStringValue(value)}");
            assignments.Append(separator);
        }

        assignments.Length -= separator.Length;
        return assignments.ToString();
    }

    private static string MapAssignment<T>(T obj, string propertyName) where T : class
    {
        return $"[{propertyName}]={GetPropertyStringValue(obj, propertyName)}";
    }

    private static string GetPropertyStringValue(object obj, string propertyName)
    {
		var propertyValue = obj.GetType().GetProperty(propertyName).GetValue(obj);
		return GetStringValue(propertyValue);
    }

	private static string GetStringValue(object obj)
    {
        if (obj == null)
            return "NULL";

		var type = obj.GetType();
		if (type == typeof(string) || type == typeof(char) || type == typeof(DateTime) || type.IsEnum)
            return $"'{obj.ToString().Replace("'", "''")}'";

		return obj.ToString();
    }
}
