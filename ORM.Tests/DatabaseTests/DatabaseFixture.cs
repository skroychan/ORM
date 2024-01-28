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

		Database.AddMapping<Person>()
			.Ignore(x => x.Contacts);
		Database.AddMapping<ContactInfo>()
			.AddForeignKey(x => x.PersonId, typeof(Person));

		Database.Initialize();
	}


	public void Dispose()
	{
		Database.DropTable(nameof(ContactInfo));
		Database.DropTable(nameof(Person));
	}
}
