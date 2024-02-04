namespace skroy.ORM.Tests.Entities;

public class Person
{
	public long Id { get; set; }
	public string Name { get; set; }
	public Gender Gender { get; set; }
	public DateTime? DateOfBirth { get; set; }
	public List<ContactInfo> Contacts { get; set; }
}
