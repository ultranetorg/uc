namespace Uccs.Net;

public class AccountIdentifier
{
	public AccountAddress	Address { get; set; }
	public AutoId			Id { get; set; }

	public static implicit operator AccountAddress(AccountIdentifier d) => d.Address;
	public static implicit operator AutoId(AccountIdentifier d) => d.Id;

	public AccountIdentifier()
	{
	}

	public AccountIdentifier(AccountAddress addres)
	{
		Address = addres;
	}

	public AccountIdentifier(AutoId id)
	{
		Id = id;
	}

	public static AccountIdentifier Parse(string text)
	{
		return text.StartsWith("0x") ? new AccountIdentifier(AccountAddress.Parse(text)) : new AccountIdentifier(AutoId.Parse(text));
	}

}
 