using skroy.ORM.Tests.Entities;
using Xunit;

namespace skroy.ORM.Tests.DatabaseTests;

public class DatabaseTest : IClassFixture<DatabaseFixture>, IDisposable
{
	public readonly Database Database;


	public DatabaseTest(DatabaseFixture databaseFixture)
	{
		Database = databaseFixture.Database;
	}


	[Fact]
	public void Insert()
	{
		var person = new Person { Name = "skroy", Gender = Gender.NonBinary, DateOfBirth = new DateTime(1990, 1, 1) };
		var personId = Database.Insert(person);
		Assert.Equal(typeof(long), personId.GetType());

		var contactInfo = new ContactInfo { Email = "skroy@example.com", PersonId = (long)personId, Phone = "+1234567890", Latitude = 12.345, Longitude = -76.54321 };
		var contactInfoId = Database.Insert(contactInfo);
		Assert.Equal(typeof(long), contactInfoId.GetType());

		var dbPersons = Database.Select<Person>();
		var dbContactInfos = Database.Select<ContactInfo>();
		var dbPerson = Assert.Single(dbPersons);
		var dbContactInfo = Assert.Single(dbContactInfos);
		Assert.Equal(personId, dbPerson.Id);
		Assert.Equal(contactInfoId, dbContactInfo.Id);
	}

	[Fact]
	public void Insert_Null()
	{
		var person = new Person();
		var personId = Database.Insert(person);
		Assert.Equal(typeof(long), personId.GetType());

		var contactInfo = new ContactInfo { PersonId = (long)personId };
		var contactInfoId = Database.Insert(contactInfo);
		Assert.Equal(typeof(long), contactInfoId.GetType());

		var dbPersons = Database.Select<Person>();
		var dbContactInfos = Database.Select<ContactInfo>();
		var dbPerson = Assert.Single(dbPersons);
		var dbContactInfo = Assert.Single(dbContactInfos);
		Assert.Equal(personId, dbPerson.Id);
		Assert.Equal(contactInfoId, dbContactInfo.Id);
		Assert.Null(dbPerson.Name);
		Assert.Null(dbContactInfo.Email);
		Assert.Null(dbContactInfo.Phone);
	}

	[Fact]
	public void Insert_Multiple()
	{
		var person1 = new Person { Name = "Jane Doe" };
		var person2 = new Person { Name = "John Doe" };

		var person1Id = Database.Insert(person1);
		var person2Id = Database.Insert(person2);

		Assert.Equal(typeof(long), person1Id.GetType());
		Assert.Equal(typeof(long), person2Id.GetType());

		var dbPersons = Database.Select<Person>();
		var dbPerson1 = dbPersons.First(x => x.Id == (long)person1Id);
		Assert.Equal(person1.Name, dbPerson1.Name);
		var dbPerson2 = dbPersons.First(x => x.Id == (long)person2Id);
		Assert.Equal(person2.Name, dbPerson2.Name);
	}

	[Fact]
	public void Select_Specific()
	{
		var person = new Person { Name = "Test", Gender = Gender.Other, DateOfBirth = new DateTime(1990, 1, 1) };
		person.Id = (long)Database.Insert(person);

		var dbPerson = Database.Select(person);

		Assert.Equal(person.Id, dbPerson.Id);
		Assert.Equal(person.Name, dbPerson.Name);
		Assert.Equal(person.Gender, dbPerson.Gender);
		Assert.Equal(person.DateOfBirth, dbPerson.DateOfBirth);
	}

	[Fact]
	public void Select_All()
	{
		var person1Id = (long)Database.Insert(new Person());
		var person2Id = (long)Database.Insert(new Person());

		var dbPersons = Database.Select<Person>();

		var dbIds = dbPersons.Select(x => x.Id);
		Assert.Equal(2, dbIds.Count());
		Assert.Contains(person1Id, dbIds);
		Assert.Contains(person2Id, dbIds);
	}

	[Fact]
	public void Update()
	{
		var person = new Person { Name = "John", Gender = Gender.Male, DateOfBirth = new DateTime(1990, 1, 1) };
		person.Id = (long)Database.Insert(person);
		var dbPerson = Database.Select(person);
		dbPerson.Name = "Jane";
		dbPerson.Gender = Gender.Female;
		dbPerson.DateOfBirth = new DateTime(1999, 1, 9);

		var updatedCount = Database.Update(dbPerson);

		Assert.Equal(1, updatedCount);
		var updatedPerson = Database.Select(dbPerson);
		Assert.Equal(dbPerson.Name, updatedPerson.Name);
		Assert.Equal(dbPerson.Gender, updatedPerson.Gender);
		Assert.Equal(dbPerson.DateOfBirth, updatedPerson.DateOfBirth);
	}

	[Fact]
	public void Update_SpecificFields()
	{
		var person = new Person { Name = "Old", Gender = Gender.Female, DateOfBirth = new DateTime(1990, 1, 1) };
		person.Id = (long)Database.Insert(person);
		var dbPerson = Database.Select(person);

		var updatedCount = Database.Update(dbPerson, _ => new Person { Name = "New" });
		Assert.Equal(1, updatedCount);

		var updatedPerson = Database.Select(dbPerson);
		Assert.Equal("New", updatedPerson.Name);
		Assert.Equal(dbPerson.Gender, updatedPerson.Gender);
		Assert.Equal(dbPerson.DateOfBirth, updatedPerson.DateOfBirth);
	}

	[Fact]
	public void Delete()
	{
		var person = new Person();
		person.Id = (long)Database.Insert(person);

		var affectedRows = Database.Delete(person);

		Assert.Equal(1, affectedRows);
		Assert.Empty(Database.Select<Person>());
	}

	[Fact]
	public void Delete_Predicate()
	{
		var person1 = new Person();
		var person2 = new Person();
		var person3 = new Person();
		person1.Id = (long)Database.Insert(person1);
		person2.Id = (long)Database.Insert(person2);
		person3.Id = (long)Database.Insert(person3);
		var lambda = () => person3.Id;

		var affectedRows = Database.Delete<Person>(x => person1.Id == x.Id);
		affectedRows += Database.Delete<Person>(x => x.Id == person2.Id);
		affectedRows += Database.Delete<Person>(x => x.Id == lambda());

		Assert.Equal(3, affectedRows);
		Assert.Empty(Database.Select<Person>());
	}

	[Fact]
	public void Delete_NoEntries()
	{
		var person = new Person();

		var affectedRows = Database.Delete(person);

		Assert.Equal(0, affectedRows);
	}

	[Fact]
	public void Delete_NoEntries_Predicate()
	{
		var person = new Person();

		var affectedRows = Database.Delete<Person>(x => x.Id == 0);

		Assert.Equal(0, affectedRows);
	}


	public void Dispose()
	{
		Database.Truncate(nameof(ContactInfo));
		Database.Truncate(nameof(Person));
	}
}
