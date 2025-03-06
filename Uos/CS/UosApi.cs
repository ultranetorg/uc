using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos;

public class UosApiServer : JsonServer
{
	Uos Uos;

	public UosApiServer(Uos uos, Flow flow) : base(uos.Settings.Api, ApiClient.CreateOptions(), flow)
	{
		Uos = uos;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(UosApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(NodeApc)).GetType(typeof(McvApc).Namespace + '.' + call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is UosApc u) 
			return u.Execute(Uos, request, response, flow);

		throw new ApiCallException("Unknown call");
	}
}

public class UosApiClient : ApiClient
{
	//public PackageInfo			GetPackage(AprvAddress address, Flow flow) => Request<PackageInfo>(new PackageGetApc {Address = address}, flow);

	static UosApiClient()
	{
	}

	public UosApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
	{
	}

	public UosApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
	{
	}
}

public abstract class UosApc : Apc
{
	public abstract object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public class PropertyApc : UosApc
{
	public string Path { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		object o = uos;

		foreach(var i in Path.Split('.'))
		{
			o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);

			if(o == null)
				throw new NodeException(NodeError.NotFound);
		}

		switch(o)
		{
			case byte[] b:
				return b.ToHex();

			default:
				return o?.ToString();
		}
	}
}

public class RunNodeApc : UosApc
{
	public string	Net { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			uos.RunNode(Net);

		return null;
	}
}

public class NodeInfoApc : UosApc
{
	public string	Net { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			return uos.Nodes.Find(i => i.Node.Net.Address == Net);
	}
}

public class AddWalletApc : UosApc
{
	public byte[]	Raw { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			uos.Vault.AddWallet(Raw);
		
		return null;
	}
}

public class WalletsApc : UosApc
{
	public class WalletApe
	{
 		public string			Name { get; set; }
		public bool				Locked  { get; set; }
		public AccountAddress[]	Accounts { get; set; }
	}

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			 return uos.Vault.Wallets.Select(i => new WalletApe {Name = i.Name,
																 Locked = i.Locked,
																 Accounts = i.Accounts.Select(i => i.Address).ToArray()}).ToArray();
	}
}

public class UnlockWalletApc : UosApc
{
	public string	Name { get; set; } ///  Null means first
	public string	Password { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			(Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).Unlock(Password);
		}

		return null;
	}
}

public class LockWalletApc : UosApc
{
	public string	Name { get; set; } ///  Null means first

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			(Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).Lock();
		}

		return null;
	}
}

public class AddAccountToWalletApc : UosApc
{
	public string	Name { get; set; } ///  Null means first
	public byte[]	Key { get; set; } ///  Null means create new

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{	
			var a = (Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).AddAccount(Key);
		
			return a.Key.PrivateKey;
		}
	}
}

public class FindAuthenticationApc : UosApc
{
	public string			Net { get; set; } /// Null means to serach among all unlocked accounts
	public AccountAddress	Account { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(Account == null)
		{
			return uos.Vault.UnlockedAccounts.FirstOrDefault(i => i.FindAuthentication(Net) != null)?.FindAuthentication(Net);
		}
		else
		{
			return uos.Vault.Find(Account)?.FindAuthentication(Net);
		}
	}
}


public class AuthenticateApc : UosApc
{
	public string			Net { get; set; }
	public AccountAddress	Account { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		var a = uos.AuthenticationRequested(Net, Account);

		if(a != null)
		{
			return uos.Vault.Find(a.Account).GetSession(Net, a.Trust);
		} 
		else
			return null;
	}
}

public class AuthorizeApc : UosApc
{
	public string			Net { get; set; }
	public AccountAddress	Account { get; set; }
	public byte[]			Session { get; set; }
	public byte[]			Data { get; set; }
	public Trust			Trust { get; set; }

	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		var acc = uos.Vault.Find(Account);
		
		var au = acc?.Authentications.Find(i => i.Net == Net);

		if(au?.Session == null || !au.Session.SequenceEqual(Session))
			return null;

		if(acc.Key == null)
		{
			uos.UnlockRequested(Account);
		}

		if(acc.Key == null)
			return null;

		if(Trust > au.Trust)
		{
			uos.AuthorizationRequested(Net, Account);
		}

		return uos.Settings.Rdn.Cryptography.Sign(acc.Key, Cryptography.Hash(Data)); ///TODO: CALL THE NET CLINENT ITSELF
	}
}

//public class PackageGetApc : UosApc
//{
//	public AprvAddress	Address { get; set; }
//
//	public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
//	{
//		var p = uos.Rdn.FindLocalPackage(Address, flow);
 	//		
//		if(p == null || !p.Available)
//		{
//			p = uos.Rdn.DeployPackage(Address, uos.Settings.Packages, flow);
//		}
//
//		p.Path = PackageHub.AddressToDeployment(uos.Settings.Packages, Address);
//
//		return p;
//	}
//}
