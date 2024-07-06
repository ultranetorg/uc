namespace Uccs.Net
{
	public class AccountIdentifier
	{
		public AccountAddress	Address { get; set; }
		public EntityId			Id { get; set; }

		public static implicit operator AccountAddress(AccountIdentifier d) => d.Address;
		public static implicit operator EntityId(AccountIdentifier d) => d.Id;

		public AccountIdentifier()
		{
		}

		public AccountIdentifier(AccountAddress addres)
		{
			Address = addres;
		}

		public AccountIdentifier(EntityId id)
		{
			Id = id;
		}
	}
}
