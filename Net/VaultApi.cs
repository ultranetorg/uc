namespace Uccs.Net;

public enum Trust : byte
{
	None, NonSpending, Spending
}

public class AuthenticationResult
{
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
}

public class VaultApiClient : ApiClient
{
	public VaultApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
	}
}

//public class AddWalletApc : Apc
//{
//	public byte[]	Raw { get; set; }
//}
//
//public class UnlockWalletApc : Apc
//{
//	public string	Name { get; set; } ///  Null means first
//	public string	Password { get; set; }
//}
//
//public class LockWalletApc : Apc
//{
//	public string	Name { get; set; } ///  Null means first
//}
//
//public class AddAccountToWalletApc : Apc
//{
//	public string	Name { get; set; } ///  Null means first
//	public byte[]	Key { get; set; } ///  Null means create new
//}

public class IsAuthenticatedApc : Apc
{
	public string			Net { get; set; }
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
}

public class AuthenticateApc : Apc
{
	public string			Application { get; set; }
	public string			Net { get; set; } /// rdn/fair /fair
	public AccountAddress	Account { get; set; } /// optional
}

public class AuthorizeApc : Apc
{
	public string			Net { get; set; }
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
	public byte[]			Hash { get; set; }
	public Trust			Trust { get; set; }
}
