using System.Net;
using System.Reflection;

namespace Uccs.Vault;

internal class VaultApiServer : JsonServer
{
	Vault Vault;

	public VaultApiServer(Vault vault, IpApiSettings settings, Flow workflow) : base(settings.ToApiSettings(vault.Settings.Zone, Api.Vault), VaultApiClient.CreateOptions(), workflow)
	{
		Vault = vault;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(VaultApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(NodeApc)).GetType(typeof(McvApc).Namespace + '.' + call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is IVaultApc u) 
			return u.Execute(Vault, request, response, flow);

		throw new ApiCallException("Unknown call");
	}
}

public interface IVaultApc
{
	public abstract object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public abstract class AdminApc : Apc, IVaultApc
{
	public byte[]	AdminKey { get; set; }

	public abstract object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public class AddWalletApc : AdminApc
{
	public string	Name { get; set; }
	public byte[]	Raw { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			vault.AddWallet(Name, Raw); 
		}
		
		return null;
	}
}

public class WalletsApc : AdminApc
{
	public class Wallet
	{
 		public string	Name { get; set; }
		public bool		Locked  { get; set; }
	}

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			return vault.Wallets.Select(i => new Wallet
											 {
												Name = i.Name,
												Locked = i.Locked,
											 }).ToArray();
		}
	}
}

public class WalletAccountsApc : AdminApc
{
 	public string		Name { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var w = vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null);

			return w.Accounts;
		}
	}
}

public class UnlockWalletApc : AdminApc
{
 	public string		Name { get; set; }
	public string		Password { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null).Unlock(Password);
		}

		return null;
	}
}

public class LockWalletApc : AdminApc
{
 	public string		Name { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{ 
			vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null).Lock();
		}

		return null;
	}
}

public class AddAccountToWalletApc : AdminApc
{
	public string		Name { get; set; } ///  Null means first
	public byte[]		Key { get; set; } ///  Null means create new

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var a = vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null).AddAccount(Key);
		
			return a.Key.PrivateKey;
		}
	}
}

public class OverrideAuthenticationApc : AdminApc
{
	public bool		Active { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			if(Active)
			{
				vault.AuthenticationRequested = (application, logo, net, account) => new AuthenticationChoice {Account = account, Trust = Trust.Complete};
			} 
			else
			{
				vault.AuthenticationRequested = null;
			}
		}

		return null;
	}
}

internal class IsAuthenticatedApc : Net.IsAuthenticatedApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.IsAuthenticated(Account, Application, Net, Session);
	}
}

internal class AuthenticateApc : Net.AuthenticateApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			var c = vault.AuthenticationRequested?.Invoke(Application, Logo, Net, Account);
	
			if(c != null)
			{
				var a = vault.Find(c.Account);
		
				if(a == null)
					throw new VaultException(VaultError.AccountNotFound);
		
				var n = a.AddAuthentication(Application, Net, Logo, c.Trust);
		
				return new AuthenticationResult {Account = c.Account, Session = n.Session};
			} 
			else
				return null;
		}
	}
}

internal class AuthorizeApc : Net.AuthorizeApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		var acc = vault.Find(Account);
		
		var au = acc?.Authentications?.Find(i => i.Net == Net && i.Application == Application && i.Session.SequenceEqual(Session));

		if(au == null)
			return null;

		if(acc.Key == null)
		{
			vault.UnlockRequested(Account);
		}

		if(acc.Key == null)
			return null;

		if(au.Trust == Trust.AskEveryTime)
		{
			vault.AuthorizationRequested(Account, au, Operation);
		}

		return Cryptography switch 
							{
								CryptographyType.No => Uccs.Net.Cryptography.No.Sign(acc.Key, Hash),
								CryptographyType.Mcv => Uccs.Net.Cryptography.Mcv.Sign(acc.Key, Hash),
								_ => throw new VaultException(VaultError.UnknownCtyptography)
							};
	}
}
