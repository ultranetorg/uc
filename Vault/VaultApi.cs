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
	public string		Wallet { get; set; } ///  Null means first
	public string		Name { get; set; } ///  Null means first
	public byte[]		Key { get; set; } ///  Null means create new

	public override object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var a = vault.Wallets.FirstOrDefault(i => i.Name == Wallet, vault.Wallets[0]).AddAccount(Name, Key);
		
			return a.Key.PrivateKey;
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

internal class IsAuthenticatedApc : Net.IsAuthenticatedApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.IsAuthenticated(User, Application, Net, Session);
	}
}

internal class AuthenticateApc : Net.AuthenticateApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			var c = vault.AuthenticationRequested?.Invoke(Application, Logo, Net, User, Account);
	
			if(c != null)
			{
				var a = vault.Find(c.Account);
		
				if(a == null)
					throw new VaultException(VaultError.AccountNotFound);
		
				var n = a.AddAuthentication(Application, Net, User, Logo, c.Trust);
		
				return new AuthenticationResult {Account = c.Account, Session = n.Session};
			} 
			else
				throw new VaultException(VaultError.Rejected);
		}
	}
}

internal class AuthorizeApc : Net.AuthorizeApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(string.IsNullOrWhiteSpace(Application) || string.IsNullOrWhiteSpace(Net) || Session.Length != Uccs.Net.Cryptography.HashSize)
			throw new VaultException(VaultError.IncorrectArgumets);

		var h = new	Authentication {Application = Application, Net = Net, Session = Session, User = User}.Hashify();

		WalletAccount acc;

		lock(vault)
		{
			var w = vault.Wallets.Find(i => i.AuthenticationHashes.Contains(h, Bytes.EqualityComparer));
	
			if(w == null)
				throw new VaultException(VaultError.NotFound);
	
			if(w.Locked)
				vault.UnlockRequested?.Invoke(null,w.Name);
	
			if(w.Locked)
				throw new VaultException(VaultError.Locked);
	
			//acc = w.Accounts.Find(i => i.Address == Account);
			
			acc = w.Accounts.FirstOrDefault(i => i.Authentications.Any(i =>	i.Session.SequenceEqual(Session)));
	
			var au = acc?.Authentications.Find(i => i.Session.SequenceEqual(Session));

			if(au == null)
				throw new VaultException(VaultError.Corrupted);
	
			if(au.Trust == Trust.AskEveryTime)
				vault.AuthorizationRequested(acc.Address, au, Operation);
		}

		return Cryptography switch 
							{
								CryptographyType.No => Uccs.Net.Cryptography.No.Sign(acc.Key, Hash),
								CryptographyType.Mcv => Uccs.Net.Cryptography.Mcv.Sign(acc.Key, Hash),
								_ => throw new VaultException(VaultError.UnknownCtyptography)
							};
	}
}
