using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos
{
	public class UosApiServer : JsonServer
	{
		Uos Uos;

		public UosApiServer(Uos uos, Flow workflow) : base(uos.Settings.Api, ApiClient.CreateOptions(), workflow)
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
		public abstract object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	}

	public class RunIcnApc : UosApc
	{
		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
				uos.RunIcn();

			return null;
		}
	}

	public class RunNodeApc : UosApc
	{
		public Guid		Mcvid { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
				uos.RunNode(Mcvid);

			return null;
		}
	}

	public class NodeInfoApc : UosApc
	{
		public Guid		Mcvid { get; set; }

		public override object Execute(Uos uos, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(uos)
				return uos.Nodes.Find(i => i.Node.Net.Id == Mcvid);
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
}
