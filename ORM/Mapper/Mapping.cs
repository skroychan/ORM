using skroy.ORM.Helpers;
using System.Linq.Expressions;

namespace skroy.ORM.Mapper;

public abstract class Mapping
{
    internal string TableName { get; private protected set; }
	internal List<Column> Columns { get; private protected set; }
	internal List<Index> Indices { get; private protected set; }
    internal Column PrimaryKey { get; private protected set; }
    internal Dictionary<Column, Type> ForeignKeys { get; private protected set; } = [];
}

public class Mapping<T> : Mapping where T : class
{
	internal const string defaultPrimaryKey = "Id";


    private Mapping()
    {
    }


	public class Builder
	{
		private readonly Mapping<T> mapping;


		internal Builder()
		{
			mapping = new Mapping<T>();
			mapping.TableName = typeof(T).Name;
			mapping.Columns = typeof(T).GetProperties()
				.Select(property =>
				{
					var type = TypeHelper.GetUnderlyingType(property, out var isNullable);
					return new Column(property.Name, type, isNullable);
				})
				.ToList();
			mapping.PrimaryKey = mapping.Columns.Find(x => x.Name == defaultPrimaryKey);
			mapping.Indices = new List<Index>();
		}


		internal Mapping<T> Build() => mapping;

		public Builder SetPrimaryKey<P>(Expression<Func<T, P>> selector)
		{
			var memberName = ExpessionHelper.GetMemberName(selector);
			var column = mapping.Columns.Single(x => x.Name == memberName);
			if (column.IsNullable)
				throw new ArgumentException($"Cannot set a nullable column [{column.Name}] as a primary key for table [{mapping.TableName}].");
			mapping.PrimaryKey = column;

			return this;
		}

		public Builder AddForeignKey<P>(Expression<Func<T, P>> selector, Type foreignType)
		{
			var memberName = ExpessionHelper.GetMemberName(selector);
			var column = mapping.Columns.Single(x => x.Name == memberName);
			mapping.ForeignKeys[column] = foreignType;

			return this;
		}

		public Builder AddIndex(bool isUnique, params Expression<Func<T, object>>[] selectors)
		{
			if (selectors.Length == 0)
				throw new ArgumentException("No columns specified for index.");

			var columns = new List<Column>();
			foreach (var selector in selectors)
			{
				var memberName = ExpessionHelper.GetMemberName(selector);
				var column = mapping.Columns.Single(x => x.Name == memberName);				
				columns.Add(column);
			}

			mapping.Indices.Add(new Index(columns, isUnique));

			return this;
		}

		public Builder Ignore<P>(Expression<Func<T, P>> selector)
		{
			var memberName = ExpessionHelper.GetMemberName(selector);
			mapping.Columns.RemoveAll(x => x.Name == memberName);

			return this;
		}
	}
}
