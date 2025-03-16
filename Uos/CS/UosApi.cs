using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos;

internal class UosApiServer : JsonServer
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

internal class RunNodeApc : Net.RunNodeApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			uos.RunNode(Net);

		return null;
	}
}

internal class NodeInfoApc : Net.NodeInfoApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
			return uos.Nodes.Find(i => i.Node.Net.Address == Net);
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
			 return uos.Vault.Wallets.Select(i => new WalletApe {Name = i.Name,
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

internal class AuthenticateApc : Net.AuthenticateApc, IUosApc
{
	public object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
	{
		lock(uos)
		{
			var c = uos.AuthenticationRequested(Net, Account);
	
			if(c != null)
			{
				return new AccountSession {Account = c.Account, Session = uos.Vault.Find(c.Account).GetAuthentication(Net, c.Trust).Session};
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
