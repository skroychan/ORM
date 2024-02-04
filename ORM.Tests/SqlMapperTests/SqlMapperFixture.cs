using skroy.ORM.Dialects;
using skroy.ORM.Mapper;
using skroy.ORM.Tests.Entities;

namespace skroy.ORM.Tests.SqlMapperTests;

public class SqlMapperFixture : IDisposable
{
	public ISqlMapper Mapper { get; private set; }


	public SqlMapperFixture()
	{
		Mapper = new SqlMapper(new SqliteDialect());

		var personMapping = new Mapping<Person>()
			.Ignore(x => x.Contacts);
		var contactInfoMapping = new Mapping<ContactInfo>()
			.AddForeignKey(x => x.PersonId, typeof(Person));

		Mapper.AddMapping(personMapping);
		Mapper.AddMapping(contactInfoMapping);
	}


	public void Dispose()
	{
		Mapper = null;
	}
}
