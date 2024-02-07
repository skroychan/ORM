using skroy.ORM.Mapper;
using skroy.ORM.Tests.Entities;
using Xunit;

namespace skroy.ORM.Tests;

public class MappingBuilderTest
{
	public readonly Mapping<Person>.MappingBuilder mappingBuilder;


    public MappingBuilderTest()
    {
		mappingBuilder = new Mapping<Person>.MappingBuilder();
    }


    [Fact]
	public void Default()
	{
		var mapping = mappingBuilder.Build();

		Assert.Equal("Person", mapping.TableName);
		Assert.Equal(5, mapping.Columns.Count);
		var idColumn = mapping.Columns.Single(x => x.Name == "Id");
		Assert.Equal(typeof(long), idColumn.Type);
		Assert.False(idColumn.IsNullable);
		var nameColumn = mapping.Columns.Single(x => x.Name == "Name");
		Assert.Equal(typeof(string), nameColumn.Type);
		Assert.True(nameColumn.IsNullable);
		var genderColumn = mapping.Columns.Single(x => x.Name == "Gender");
		Assert.Equal(typeof(Gender), genderColumn.Type);
		Assert.False(genderColumn.IsNullable);
		var dateColumn = mapping.Columns.Single(x => x.Name == "DateOfBirth");
		Assert.Equal(typeof(DateTime), dateColumn.Type);
		Assert.True(dateColumn.IsNullable);
		Assert.Equal("Id", mapping.PrimaryKey.Name);
	}

	[Fact]
	public void SetPrimaryKey()
	{
		var mapping = mappingBuilder
			.SetPrimaryKey(x => x.Gender)
			.Build();

		Assert.Equal("Gender", mapping.PrimaryKey.Name);
		Assert.Equal(typeof(Gender), mapping.PrimaryKey.Type);
	}

	[Fact]
	public void SetPrimaryKey_Nullable()
	{
		Assert.Throws<ArgumentException>(() => mappingBuilder.SetPrimaryKey(x => x.DateOfBirth));
	}

	[Fact]
	public void AddForeignKey()
	{
		var mapping = mappingBuilder
			.AddForeignKey(x => x.Gender, typeof(Gender))
			.AddForeignKey(x => x.DateOfBirth, typeof(ContactInfo))
			.Build();

		Assert.Equal(2, mapping.ForeignKeys.Count);
		var foreignKey = mapping.ForeignKeys.Single(x => x.Key.Name == "Gender");
		Assert.Equal(typeof(Gender), foreignKey.Key.Type);
		Assert.Equal(typeof(Gender), foreignKey.Value);
		foreignKey = mapping.ForeignKeys.Single(x => x.Key.Name == "DateOfBirth");
		Assert.Equal(typeof(DateTime), foreignKey.Key.Type);
		Assert.Equal(typeof(ContactInfo), foreignKey.Value);
	}

	[Fact]
	public void AddIndex()
	{
		var mapping = mappingBuilder
			.AddIndex(true, x => x.Name, x => x.DateOfBirth)
			.Build();

		var index = Assert.Single(mapping.Indices);
		Assert.True(index.IsUnique);
		Assert.Equal(2, index.Columns.Count());
		var column = index.Columns.Single(x => x.Name == "Name");
		Assert.Equal(typeof(string), column.Type);
		column = index.Columns.Single(x => x.Name == "DateOfBirth");
		Assert.Equal(typeof(DateTime), column.Type);
	}

	[Fact]
	public void Ignore()
	{
		var mapping = mappingBuilder
			.Ignore(x => x.Contacts)
			.Build();

		Assert.Equal(4, mapping.Columns.Count);
		var columnNames = mapping.Columns.Select(x => x.Name).ToList();
		Assert.DoesNotContain("Contacts", columnNames);
	}
}
