using skroy.ORM.Mapper;
using skroy.ORM.Tests.Entities;

namespace skroy.ORM.Tests.DatabaseTests;

public class DatabaseFixture : IDisposable
{
	public Database Database;


	public DatabaseFixture()
	{
		Database = new Database(DatabaseProvider.Sqlite, $"Data Source=notes.db;");

		Database.DropTable(nameof(ContactInfo));
		Database.DropTable(nameof(Person));

		Database.AddMapping(new Mapping<Person>.MappingBuilder()
			.Ignore(x => x.Contacts));
		Database.AddMapping(new Mapping<ContactInfo>.MappingBuilder());

		Database.Initialize();
	}


	public void Dispose()
	{
		Database.DropTable(nameof(ContactInfo));
		Database.DropTable(nameof(Person));
	}
}
