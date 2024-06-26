namespace Uccs.Rdn
{
	public class DomainIdentifier
	{
		public string			Addres { get; set; }
		public EntityId			Id { get; set; }

		public static implicit	operator string(DomainIdentifier d) => d.Addres;
		public static implicit	operator EntityId(DomainIdentifier d) => d.Id;

		public DomainIdentifier()
		{
		}

		public DomainIdentifier(string address)
		{
			Addres = address;
		}

		public DomainIdentifier(EntityId id)
		{
			Id = id;
		}
	}
}
