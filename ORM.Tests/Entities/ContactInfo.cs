namespace skroy.ORM.Tests.Entities;

public class ContactInfo
{
	public long Id { get; set; }
	public long PersonId { get; set; }
	public bool IsDeleted { get; set; }
	public string Email { get; set; }
	public string Phone { get; set; }
	public double? Latitude { get; set; }
	public double? Longitude { get; set; }
}
