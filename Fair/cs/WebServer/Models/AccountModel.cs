namespace Uccs.Fair;

public class AccountModel
{
	public string Id { get; set; }

	public string Address { get; set; }

	public AccountModel(Account account)
	{
		Id = account.Id.ToString();
		Address = account.Address.ToString();
	}
}
