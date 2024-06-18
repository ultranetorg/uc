using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos
{
	public class UosApiServer : JsonServer
	{
		Uos Uos;

		public UosApiServer(Uos uos, Flow workflow) : base(uos.Settings.Api, ApiClient.DefaultOptions, workflow)
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
				
			if(call is NodeApc s)
			{
				if(Uos.Izn == null)
					throw new NodeException(NodeError.NoIzn);
	
				return s.Execute(Uos.Izn, request, response, flow);
			}

			throw new ApiCallException("Unknown call");
		}
	}
	
	public abstract class UosApc : Apc
	{
		public abstract object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	}

	public class RunIcnApc : UosApc
	{
		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			uos.RunIcn();

			return null;
		}
	}

	public class RunMcvApc : UosApc
	{
		public Guid Mcvid { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			uos.RunNode(Mcvid);

			return null;
		}
	}

	public class AddWalletUosApc : UosApc
	{
		public byte[]	Wallet { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
				uos.Vault.AddWallet(Wallet);
			
			return null;
		}
	}

	public class SaveWalletUosApc : UosApc
	{
		public AccountAddress	Account { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
				uos.Vault.SaveWallet(Account);
			
			return null;
		}
	}

	public class UnlockWalletApc : UosApc
	{
		public AccountAddress	Account { get; set; }
		public string			Password { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
			{
				if(Account != null)
					uos.Vault.Unlock(Account, Password);
				else
					foreach(var i in uos.Vault.Wallets)
						uos.Vault.Unlock(i.Key, Password);
			}

			return null;
		}
	}

}
