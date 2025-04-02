namespace Uccs.Fair;

public class AccountBaseModel
{
	public string Id { get; set; }

	public string Address { get; set; }

	public string Nickname { get; set; }

	public AccountBaseModel(FairAccount account)
	{
		Id = account.Id.ToString();
		Address = account.Address.ToString();
		Nickname = account.Nickname;
	}
}
