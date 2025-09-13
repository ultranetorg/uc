using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Net;

public class UosApiClient : ApiClient
{
	public UosApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
	{
	}

	public UosApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
	{
	}
}

public class UosPropertyApc : Apc
{
	public string Path { get; set; }
}

public class NodeInfoApc : Apc
{
	public string	Net { get; set; }
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
	//public AccountAddress	Account { get; set; }
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
