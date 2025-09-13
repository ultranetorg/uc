using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos;

internal class UosApiServer : JsonServer
{
	Uos Uos;

	public UosApiServer(Uos uos, Flow flow) : base(	new ApiSettings
													{
														LocalAddress = UosApiSettings.GetAddress(uos.Settings.Rdn.Zone, uos.Settings.Api.LocalIP, false),
														PublicAddress = uos.Settings.Api.PublicIP == null ? null : UosApiSettings.GetAddress(uos.Settings.Rdn.Zone, uos.Settings.Api.PublicIP, uos.Settings.Api.Ssl),
														PublicAccessKey = uos.Settings.Api.PublicAccessKey,
													},
													ApiClient.CreateOptions(),
													flow)
	{
		Uos = uos;
	}

	protected override Type Create(string call)
	{
		return Type.GetType(typeof(UosApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(NodeApc)).GetType(typeof(McvApc).Namespace + '.' + call);
	}

	protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		if(call is IUosApc u) 
			return u.Execute(Uos, request, response, flow);

		throw new ApiCallException("Unknown call");
	}
}

internal interface IUosApc
{
	public abstract object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow);
}

public class UosPropertyApc : Net.UosPropertyApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
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

internal class NodeInfoApc : Net.NodeInfoApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			return uos.Nodes.Find(i => i.Net == Net);
	}
}

internal class AddWalletApc : Net.AddWalletApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			uos.Vault.AddWallet(Raw);
		
		return null;
	}
}

internal class WalletsApc : Net.WalletsApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			 return uos.Vault.Wallets.Select(i => new Wallet{Name = i.Name,
															 Locked = i.Locked,
															 Accounts = i.Accounts.Select(i => i.Address).ToArray()}).ToArray();
	}
}

internal class UnlockWalletApc : Net.UnlockWalletApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			(Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).Unlock(Password);
		}

		return null;
	}
}

internal class LockWalletApc : Net.LockWalletApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			(Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).Lock();
		}

		return null;
	}
}

internal class AddAccountToWalletApc : Net.AddAccountToWalletApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{	
			var a = (Name == null ? uos.Vault.Wallets.First() : uos.Vault.Wallets.Find(i => i.Name == Name)).AddAccount(Key);
		
			return a.Key.PrivateKey;
		}
	}
}

internal class IsAuthenticatedApc : Net.IsAuthenticatedApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			return uos.Vault.UnlockedAccounts.FirstOrDefault(i => i.Address == Account)?.FindAuthentication(Net)?.Session.SequenceEqual(Session) ?? false;
	}
}

internal class EnforceAuthenticationApc : Net.EnforceAuthenticationApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			if(Active)
			{
				uos.AuthenticationRequested = (net, account) => new AuthenticationChioce {Account = account, Trust = Trust.Spending};
			} 
			else
			{
				uos.AuthenticationRequested = null;
			}

			//if(Account != null)
			//{
			//	uos.GetMcvApi(Net).Send(new EnforceSessionsApc {Account = Account}, flow);
			//} 
			//else
			//{
			//	foreach(var w in uos.Vault.Wallets)
			//	{
			//		foreach(var i in w.Accounts)
			//		{
			//			 uos.GetMcvApi(Net).Send(new EnforceSessionsApc {Account = i.Address}, flow);
			//		}
			//	}
			//}

			//uos.AuthenticationRequested = old;
		}

		return null;
	}
}

internal class AuthenticateApc : Net.AuthenticateApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			var c = uos.AuthenticationRequested?.Invoke(Net, Account);
	
			if(c != null)
			{
				var a = uos.Vault.Find(c.Account);
		
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

internal class AuthorizeApc : Net.AuthorizeApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
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

		return uos.Vault.Cryptography.Sign(acc.Key, Hash); ///TODO: CALL THE NET CLINENT ITSELF
	}
}
