namespace Uccs.Net;

public enum Trust : byte
{
	None, NonSpending, Spending
}

public class AccountSession
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

public class AddWalletApc : Apc
{
	public byte[]	Raw { get; set; }
}

public class WalletsApc : Apc
{
	public class Wallet
	{
 		public string			Name { get; set; }
		public bool				Locked  { get; set; }
		public AccountAddress[]	Accounts { get; set; }
	}
}

public class UnlockWalletApc : Apc
{
	public string	Name { get; set; } ///  Null means first
	public string	Password { get; set; }
}

public class LockWalletApc : Apc
{
	public string	Name { get; set; } ///  Null means first
}

public class AddAccountToWalletApc : Apc
{
	public string	Name { get; set; } ///  Null means first
	public byte[]	Key { get; set; } ///  Null means create new
}

public class IsAuthenticatedApc : Apc
{
	public string			Net { get; set; }
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
}

public class EnforceAuthenticationApc : Apc
{
	public bool				Active { get; set; }
}

public class AuthenticateApc : Apc
{
	public string			Net { get; set; }
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
