using skroy.ORM.Mapper;
using skroy.ORM.Tests.Entities;
using Xunit;

namespace skroy.ORM.Tests.SqlMapperTests;

public class SqlMapperTest : IClassFixture<SqlMapperFixture>
{
	private readonly ISqlMapper Mapper;


	public SqlMapperTest(SqlMapperFixture sqlMapperFixture)
	{
		Mapper = sqlMapperFixture.Mapper;
	}


	[Fact]
	public void GetColumns()
	{
		var columns = Mapper.GetColumns<Person>();

		Assert.Equal(4, columns.Count());
		var columnNames = columns.Select(x => x.Name).ToList();
		Assert.Contains("Id", columnNames);
		Assert.Contains("Name", columnNames);
		Assert.Contains("Gender", columnNames);
		Assert.Contains("DateOfBirth", columnNames);
	}

	[Fact]
	public void Create()
	{
		var query = Mapper.MapCreate();

		Assert.Contains("create table if not exists [Person] " +
			"([Id] integer primary key autoincrement not null unique," +
			"[Name] text," +
			"[Gender] text not null," +
			"[DateOfBirth] text);", query);

		Assert.Contains("create table if not exists [ContactInfo] " +
			"([Id] integer primary key autoincrement not null unique," +
			"[PersonId] integer not null," +
			"[Email] text," +
			"[Phone] text," +
			"[Latitude] real," +
			"[Longitude] real," +
			"foreign key([PersonId]) references [Person]([Id]));", query);
	}

	[Theory]
	[InlineData(true, "create unique index idx_Name on [Person] ([Name]);")]
	[InlineData(false, "create index idx_Name on [Person] ([Name]);")]
	public void Create_Indices(bool isUnique, string expectedQuery)
	{
		var mappingBuilder = new Mapping<Person>.MappingBuilder();
		mappingBuilder.AddIndex(isUnique, x => x.Name);
		Mapper.AddMapping(mappingBuilder.Ignore(x => x.Contacts));

		var query = Mapper.MapCreate();

		Assert.Contains(expectedQuery, query);
	}

	[Fact]
	public void Create_CompositeIndices()
	{
		var mappingBuilder = new Mapping<Person>.MappingBuilder();
		mappingBuilder.AddIndex(true, x => x.Name, x => x.Gender);
		Mapper.AddMapping(mappingBuilder.Ignore(x => x.Contacts));

		var query = Mapper.MapCreate();

		Assert.Contains("create unique index idx_Name_Gender on [Person] ([Name],[Gender]);", query);
	}

	[Fact]
	public void Insert()
	{
		var person = new Person { Name = "skroy", Gender = Gender.NonBinary, DateOfBirth = new DateTime(2002, 2, 2) };

		var query = Mapper.MapInsert(person);

		Assert.Contains("[Person]", query);
		Assert.DoesNotContain("[Id]", query);
		Assert.Contains("[Name]", query);
		Assert.Contains("[Gender]", query);
		Assert.Contains("[DateOfBirth]", query);
		Assert.Contains("'skroy'", query);
		Assert.Contains("'NonBinary'", query);
		Assert.Contains("'2/2/2002", query);
	}

	[Fact]
	public void Insert_Null()
	{
		var person = new Person { Name = "Niel" };

		var query = Mapper.MapInsert(person);

		Assert.Contains("[Person]", query);
		Assert.DoesNotContain("[Id]", query);
		Assert.Contains("[Name]", query);
		Assert.Contains("[Gender]", query);
		Assert.Contains("[DateOfBirth]", query);
		Assert.Contains("'Niel'", query);
		Assert.Contains("'Unspecified'", query);
		Assert.Contains("NULL", query);
	}

	[Fact]
	public void Insert_Multiple()
	{
		var person1 = new Person { Name = "Hitori", Gender = Gender.Male, DateOfBirth = new DateTime(2000, 1, 1) };
		var person2 = new Person { Name = "Futari", Gender = Gender.Female, DateOfBirth = new DateTime(1999, 12, 31) };

		var query = Mapper.MapInsert(person1, person2);

		Assert.Contains("[Person]", query);
		Assert.DoesNotContain("[Id]", query);
		Assert.Contains("[Name]", query);
		Assert.Contains("[Gender]", query);
		Assert.Contains("[DateOfBirth]", query);
		Assert.Contains("'Hitori','Male'", query);
		Assert.Contains("1/1/2000", query);
		Assert.Contains("),('Futari','Female'", query);
		Assert.Contains("12/31/1999", query);
	}

	[Fact]
	public void Select_All()
	{
		var query = Mapper.MapSelect<Person>();

		Assert.Equal("select * from [Person];", query);
	}

	[Fact]
	public void Select_Specific()
	{
		var person = new Person { Id = 1 };

		var query = Mapper.MapSelect(person);

		Assert.Equal("select * from [Person] where [Id]=1;", query);
	}

	[Fact]
	public void Update()
	{
		var person = new Person { Id = 1, Name = "Tooth", Gender = Gender.NonBinary };

		var query = Mapper.MapUpdate(person);

		Assert.Equal("update [Person] set [Name]='Tooth',[Gender]='NonBinary',[DateOfBirth]=NULL where [Id]=1;", query);
	}

	[Fact]
	public void Update_SpecificFields()
	{
		var person = new Person { Id = 2 };

		var query = Mapper.MapUpdate(person, _ => new Person { Name = "Nail", Gender = Gender.Other });

		Assert.Equal("update [Person] set [Name]='Nail',[Gender]='Other' where [Id]=2;", query);
	}

	[Fact]
	public void Delete()
	{
		var person = new Person { Id = 3 };

		var query = Mapper.MapDelete(person);

		Assert.Equal("delete from [Person] where [Id]=3;", query);
	}

	[Fact]
	public void Delete_Predicate()
	{
		var query = Mapper.MapDelete<Person>(x => x.Id == 3);

		Assert.Equal("delete from [Person] where [Id]=3;", query);
	}
}
