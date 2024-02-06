using System.Linq.Expressions;
using skroy.ORM.Helpers;

namespace skroy.ORM.Mapper;

public abstract class Mapping
{
    internal string TableName { get; private protected set; }
	internal List<Column> Columns { get; private protected set; }
    internal Column PrimaryKey { get; private protected set; }
    internal Dictionary<Column, Type> ForeignKeys { get; private protected set; } = [];
}

public class Mapping<T> : Mapping where T : class
{
	internal const string defaultPrimaryKey = "Id";


    private Mapping()
    {
    }


	public class MappingBuilder
	{
		private readonly Mapping<T> mapping;


		internal MappingBuilder()
		{
			mapping = new Mapping<T>();
			mapping.TableName = typeof(T).Name;
			mapping.Columns = typeof(T).GetProperties()
				.Select(property =>
				{
					var type = TypeHelper.GetUnderlyingType(property, out var isNullable);
					return new Column(property.Name, type, isNullable);
				}).ToList();
			mapping.PrimaryKey = mapping.Columns.Find(x => x.Name == defaultPrimaryKey);
		}


		internal Mapping<T> Build() => mapping;

		public MappingBuilder SetPrimaryKey<P>(Expression<Func<T, P>> selector)
		{
			var expression = (MemberExpression)selector.Body;
			mapping.PrimaryKey = mapping.Columns.Single(x => x.Name == expression.Member.Name);

			return this;
		}

		public MappingBuilder AddForeignKey<P>(Expression<Func<T, P>> selector, Type foreignType)
		{
			var expression = (MemberExpression)selector.Body;
			var column = mapping.Columns.Single(x => x.Name == expression.Member.Name);
			mapping.ForeignKeys[column] = foreignType;

			return this;
		}

		public MappingBuilder Ignore<P>(Expression<Func<T, P>> selector)
		{
			var expression = (MemberExpression)selector.Body;
			mapping.Columns.RemoveAll(x => x.Name == expression.Member.Name);

			return this;
		}
	}
}
