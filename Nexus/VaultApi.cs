using System.Net;
using System.Reflection;

namespace Uccs.Nexus;

internal class VaultApiServer : JsonServer
{
	Vault Vault;

	public VaultApiServer(Vault vault, IpApiSettings settings, Flow workflow) : base(settings.ToSystemSettings(vault.Zone, Api.Vault), NetJsonConfiguration.CreateOptions(), workflow)
	{
		Vault = vault;
		
		Restricted.Add(Apc.NameOf(typeof(AddWalletApc)));
		Restricted.Add(Apc.NameOf(typeof(WalletsApc)));
		Restricted.Add(Apc.NameOf(typeof(WalletAccountsApc)));
		Restricted.Add(Apc.NameOf(typeof(UnlockWalletApc)));
		Restricted.Add(Apc.NameOf(typeof(LockWalletApc)));
		Restricted.Add(Apc.NameOf(typeof(AddAccountToWalletApc)));
		Restricted.Add(Apc.NameOf(typeof(OverrideAuthenticationApc)));
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

public class AddWalletApc : Apc, IVaultApc
{
	public string	Name { get; set; }
	public byte[]	Raw { get; set; }

	public  object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			vault.AddWallet(Name, Raw); 
		}
		
		return null;
	}
}

public class WalletsApc : Apc, IVaultApc
{
	public class Wallet
	{
 		public string	Name { get; set; }
		public bool		Locked  { get; set; }
	}

	public  object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
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

public class WalletAccountsApc : Apc, IVaultApc
{
	public class Account
	{
		public string				Name { get; set; } 
		public AccountAddress		Address { get; set; }

		public Account(WalletAccount account)
		{
			Name = account.Name;
			Address = account.Address;
		}

		public Account()
		{
  		}
	}

 	public string		Name { get; set; }

	public  object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var w = vault.Wallets.FirstOrDefault(i => i.Name == Name, vault.Wallets.FirstOrDefault());

			if(w == null)
				throw new VaultException(VaultError.NotFound);

			if(w.Locked)
				throw new VaultException(VaultError.Locked);

			return w.Accounts.Select(i => new Account(i)).ToArray();
		}
	}
}

public class UnlockWalletApc : Apc, IVaultApc
{
 	public string		Name { get; set; }
	public string		Password { get; set; }

	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null).Unlock(Password);
		}

		return null;
	}
}

public class LockWalletApc : Apc, IVaultApc
{
 	public string		Name { get; set; }

	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{ 
			var w = vault.Wallets.FirstOrDefault(i => i.Name == Name, Name == null ? vault.Wallets[0] : null)
					??
					throw new VaultException(VaultError.NotFound);
			
			w.Lock();
		}

		return null;
	}
}

public class AddAccountToWalletApc : Apc, IVaultApc
{
	public string		Wallet { get; set; } ///  Null means first
	public string		Name { get; set; } ///  Null means first
	public string		Tag { get; set; }
	public byte[]		Key { get; set; } ///  Null means create a new

	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var w = Wallet == null ? vault.Wallets.FirstOrDefault() : vault.FindWallet(Wallet);

			if(w == null)
				throw new VaultException(VaultError.NotFound);

			var a = w.AddAccount(Name, Key, Tag);
		
			return a.Address;
		}
	}
}

public class OverrideAuthenticationApc : Apc, IVaultApc
{
	public bool			Active { get; set; }

	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			if(Active)
			{
				vault.AuthenticationRequested = (application, logo, net, user, account) =>
												{	
													lock(vault)
														return new AuthenticationChoice {Account = vault.Find(user).Address, Trust = Trust.AlwaysAllow};
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
