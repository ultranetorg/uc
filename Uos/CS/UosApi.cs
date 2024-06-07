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
			return Type.GetType(typeof(UosApiServer).Namespace + '.' + call) ?? Assembly.GetAssembly(typeof(SunApc)).GetType(typeof(McvApc).Namespace + '.' + call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
		{
			if(call is UosApc u) 
				return u.Execute(Uos, request, response, flow);
				
			if(call is SunApc s)
			{
				if(Uos.Sun == null)
					throw new NodeException(NodeError.NoMcv);

				return s.Execute(Uos.Sun, request, response, flow);
			}

			if(call is McvApc m)
			{
				var mcv = Uos.Sun?.FindMcv(m.Mcvid);

				if(mcv == null)
					throw new NodeException(NodeError.NoMcv);

				return m.Execute(mcv, request, response, flow);
			}

			throw new ApiCallException("Unknown call");
		}
	}
	
	public abstract class UosApc : Apc
	{
		public abstract object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	}

	public class RunSunApc : UosApc
	{
		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			uos.RunSun();

			return null;
		}
	}

	public class RunMcvApc : UosApc
	{
		public Guid Mcvid { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			uos.RunMcv(Mcvid);

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

	public class UnlockWalletUosApc : UosApc
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
