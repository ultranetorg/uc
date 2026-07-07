using System.Net;
using System.Reflection;

namespace Uccs.Nexus;

internal class VaultApiServer : JsonServer
{
	Vault Vault;

	public VaultApiServer(Vault vault, IpApiSettings settings, Flow workflow) : base(settings.ToSystemSettings(vault.Zone, Api.Vault), NetJsonConfiguration.CreateOptions(), workflow)
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
	public string		Wallet { get; set; } ///  Null means first
	public string		Name { get; set; } ///  Null means first
	public string		Tag { get; set; } ///  Null means first
	public byte[]		Key { get; set; } ///  Null means create new

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var w = Name == null ? vault.Wallets.FirstOrDefault() : vault.FindWallet(Wallet);

			if(w == null)
				throw new VaultException(VaultError.NotFound);

			var a = w.AddAccount(Name, Key, Tag);
		
			return a.Key.Secret;
		}
	}
}

public class OverrideAuthenticationApc : AdminApc
{
	public bool			Active { get; set; }

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			if(Active)
			{
				vault.AuthenticationRequested = (application, logo, net, user, account) =>
												{	
													lock(vault)
														return new AuthenticationChoice {Account = vault.Find(user).Address, Trust = Trust.Complete};
												};
			} 
			else
			{
				vault.AuthenticationRequested = null;
			}
		}

		return null;
	}
}

internal class IsAuthenticatedApc : Uccs.Net.IsAuthenticatedApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.IsAuthenticated(User, Application, Net, Session);
	}
}

internal class AuthenticateApc : Uccs.Net.AuthenticateApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.Authenticate(Application, Net, User, Logo, Account, flow);
	}
}

internal class AuthorizeApc : Uccs.Net.AuthorizeApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.Authorize(Cryptography, Net, Operation, User, Session, Hash, flow);
	}
}
