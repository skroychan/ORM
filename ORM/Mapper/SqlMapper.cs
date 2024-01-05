using skroy.ORM.Dialects;
using System.Linq.Expressions;
using System.Text;

namespace skroy.ORM.Mapper;

public class SqlMapper
{
    private readonly Dialect dialect;
    private readonly Dictionary<Type, Mapping> Mappings;


    public SqlMapper(Dialect sqlDialect)
    {
        dialect = sqlDialect;
        Mappings = [];
    }


    public void AddMapping<T>(Mapping<T> mapping) where T : class
    {
        Mappings.Add(typeof(T), mapping);
    }

    public List<Column> GetColumns(Type mapping)
    {
        return Mappings[mapping].Columns;
    }

    public string MapCreate()
    {
        var sb = new StringBuilder();
        foreach (var mapping in Mappings.Values)
        {
            sb.Append($"create table if not exists {mapping.TableName} (");

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
                    throw new ArgumentException($"Mapping of referenced table of type {foreignKey.Value} doesn't exist");

                sb.Append("foreign key(")
                    .Append(foreignKey.Key.Name)
                    .Append(") references ")
                    .Append(referencedTable.TableName)
                    .Append("([").Append(referencedTable.PrimaryKey.Name).Append("])")
                    .Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(");");
        }

        return sb.ToString();
    }

    public string MapInsert<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        var columnNames = string.Join(',', mapping.Columns.Select(x => $"[{x.Name}]"));
        var values = string.Join(',', mapping.Columns.Select(column => GetPropertyStringValue(obj, column.Name)));
        return $"insert into {mapping.TableName} ({columnNames}) values ({values}); {dialect.SelectLastRow};";
    }

    public string MapSelect<T>() where T : class
    {
        var mapping = GetMapping<T>();
        return $"select * from {mapping.TableName};";
    }

    public string MapSelect<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"select * from {mapping.TableName} where {MapPrimaryKeyCondition(obj, mapping)};";
    }

    public string MapUpdate<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"update {mapping.TableName} set {MapSet(mapping, obj)} where {MapPrimaryKeyCondition(obj, mapping)};";
    }

    public string MapUpdate<T>(T obj, Expression<Action<T>> memberInitializer) where T : class
    {
        var mapping = GetMapping<T>();
        return $"update {mapping.TableName} set {MapSet(mapping, memberInitializer)} where {MapPrimaryKeyCondition(obj, mapping)};";
    }

    public string MapDelete<T>(T obj) where T : class
    {
        var mapping = GetMapping<T>();
        return $"delete from {mapping.TableName} where {MapPrimaryKeyCondition(obj, mapping)};";
    }


    private Mapping<T> GetMapping<T>() where T : class
    {
        if (!Mappings.TryGetValue(typeof(T), out var mapping))
            throw new ArgumentException($"Cannot find mapping for {typeof(T)}");

        return (Mapping<T>)mapping;
    }

    private string MapPrimaryKeyCondition<T>(T obj, Mapping<T> mapping) where T : class
    {
        return MapAssignment(obj, mapping.PrimaryKey.Name);
    }

    private string MapSet<T>(Mapping<T> mapping, T obj) where T : class
    {
        const string separator = ",";
        var assignments = new StringBuilder();
        foreach (var column in mapping.Columns)
            assignments.Append(MapAssignment(obj, column.Name)).Append(separator);
        assignments.Length -= separator.Length;
        return assignments.ToString();
    }

    private string MapSet<T>(Mapping<T> mapping, Expression<Action<T>> memberInitializer) where T : class
    {
        const string separator = ",";
        var assignments = new StringBuilder();
        var expression = (MemberInitExpression)memberInitializer.Body;
        foreach (var binding in expression.Bindings)
        {
            var column = binding.Member.Name;
            if (!mapping.Columns.Any(x => x.Name == column))
                continue;

            var assignment = ((MemberAssignment)binding).Expression;
            var value = Expression.Lambda(assignment).Compile().DynamicInvoke();
            assignments.Append($"{column}={GetStringValue(value)}");
            assignments.Append(separator);
        }

        assignments.Length -= separator.Length;
        return assignments.ToString();
    }

    private string MapAssignment<T>(T obj, string propertyName) where T : class
    {
        return $"[{propertyName}]={GetPropertyStringValue(obj, propertyName)}";
    }

    private string GetPropertyStringValue(object obj, string propertyName)
    {
        return GetStringValue(GetProperty(obj, propertyName));
    }

    private static object GetProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName).GetValue(obj);
    }

    private static string GetStringValue(object obj)
    {
        return obj == null ? "NULL" : IsString(obj.GetType()) ? $"'{obj}'" : obj.ToString();
    }

    private static bool IsString(Type type)
    {
        return type == typeof(string) || type == typeof(char);
    }
}
