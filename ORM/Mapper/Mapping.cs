using System.Linq.Expressions;
using skroy.ORM.Helpers;

namespace skroy.ORM.Mapper;

public abstract class Mapping
{
    public string TableName { get; protected set; }
    public List<Column> Columns { get; protected set; }
    public Column PrimaryKey { get; protected set; }
    public Dictionary<Column, Type> ForeignKeys { get; protected set; } = new();
}

public class Mapping<T> : Mapping where T : class
{
    private const string defaultPrimaryKey = "Id";


    public Mapping()
    {
        TableName = typeof(T).Name;
        Columns = typeof(T).GetProperties()
            .Select(property =>
            {
                var type = TypeHelper.GetUnderlyingType(property, out var isNullable);
                return new Column(property.Name, type, isNullable);
            }).ToList();
        PrimaryKey = Columns.Find(x => x.Name == defaultPrimaryKey);
    }


    public Mapping<T> SetPrimaryKey<P>(Expression<Func<T, P>> selector)
    {
        var expression = (MemberExpression)selector.Body;
        PrimaryKey = Columns.Single(x => x.Name == expression.Member.Name);

        return this;
    }

    public Mapping<T> AddForeignKey<P>(Expression<Func<T, P>> selector, Type foreignType)
    {
        var expression = (MemberExpression)selector.Body;
        var column = Columns.Single(x => x.Name == expression.Member.Name);
        ForeignKeys[column] = foreignType;

        return this;
    }

    public Mapping<T> Ignore<P>(Expression<Func<T, P>> selector)
    {
        var expression = (MemberExpression)selector.Body;
        Columns.RemoveAll(x => x.Name == expression.Member.Name);

        return this;
    }
}
