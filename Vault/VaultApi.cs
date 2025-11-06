using System.Net;
using System.Reflection;

namespace Uccs.Vault;

internal class VaultApiServer : JsonServer
{
	Vault Vault;

	public VaultApiServer(Vault vault, IpApiSettings settings, Flow workflow) : base(settings.ToApiSettings(vault.Settings.Zone, KnownSystem.VaultApi), VaultApiClient.CreateOptions(), workflow)
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

internal interface IVaultApc
{
	public abstract object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

internal class AddWalletApc : Net.AddWalletApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			vault.AddWallet(Raw);
		
		return null;
	}
}

internal class WalletsApc : Net.WalletsApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			 return vault.Wallets.Select(i => new Wallet{Name = i.Name,
														 Locked = i.Locked,
														 Accounts = i.Accounts.Select(i => i.Address).ToArray()}).ToArray();
	}
}

internal class UnlockWalletApc : Net.UnlockWalletApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			(Name == null ? vault.Wallets.First() : vault.Wallets.Find(i => i.Name == Name)).Unlock(Password);
		}

		return null;
	}
}

internal class LockWalletApc : Net.LockWalletApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			(Name == null ? vault.Wallets.First() : vault.Wallets.Find(i => i.Name == Name)).Lock();
		}

		return null;
	}
}

internal class AddAccountToWalletApc : Net.AddAccountToWalletApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{	
			var a = (Name == null ? vault.Wallets.First() : vault.Wallets.Find(i => i.Name == Name)).AddAccount(Key);
		
			return a.Key.PrivateKey;
		}
	}
}

internal class IsAuthenticatedApc : Net.IsAuthenticatedApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
			return vault.UnlockedAccounts.FirstOrDefault(i => i.Address == Account)?.FindAuthentication(Net)?.Session.SequenceEqual(Session) ?? false;
	}
}

internal class EnforceAuthenticationApc : Net.EnforceAuthenticationApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			if(Active)
			{
				vault.AuthenticationRequested = (net, account) => new AuthenticationChoice {Account = account, Trust = Trust.Spending};
			} 
			else
			{
				vault.AuthenticationRequested = null;
			}

			//if(Account != null)
			//{
			//	vault.GetMcvApi(Net).Send(new EnforceSessionsApc {Account = Account}, flow);
			//} 
			//else
			//{
			//	foreach(var w in vault.Wallets)
			//	{
			//		foreach(var i in w.Accounts)
			//		{
			//			 vault.GetMcvApi(Net).Send(new EnforceSessionsApc {Account = i.Address}, flow);
			//		}
			//	}
			//}

			//vault.AuthenticationRequested = old;
		}

		return null;
	}
}

internal class AuthenticateApc : Net.AuthenticateApc, IVaultApc
{
	public object Execute(Vault vault, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(vault)
		{
			var c = vault.AuthenticationRequested?.Invoke(Net, Account);
	
			if(c != null)
			{
				var a = vault.Find(c.Account);
		
				if(a == null)
					throw new VaultException(VaultError.AccountNotFound);
		
				var n = a.GetAuthentication(Net, c.Trust);
		
				if(n == null)
					throw new VaultException(VaultError.NetNotFound);
		
				return new AccountSession {Account = c.Account, Session = n.Session};
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
		
		var au = acc?.Authentications.Find(i => i.Net == Net);

		if(au?.Session == null || !au.Session.SequenceEqual(Session))
			return null;

		if(acc.Key == null)
		{
			vault.UnlockRequested(Account);
		}

		if(acc.Key == null)
			return null;

		if(Trust > au.Trust)
		{
			vault.AuthorizationRequested(Net, Account);
		}

		return vault.Cryptography.Sign(acc.Key, Hash);
	}
}
