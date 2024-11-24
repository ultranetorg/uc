using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Fair
{
	public abstract class FairApc : McvApc
	{
		public abstract object Execute(FairNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return Execute(mcv as FairNode, request, response, workflow);
		}
	}

	public class FairTypeResolver : ApiTypeResolver
	{
	    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
	    {
	        var ti = base.GetTypeInfo(type, options);

	        if(ti.Type == typeof(PeerRequest))
	        {
				foreach(var i in typeof(FairPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
				{
					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }

	        if(ti.Type == typeof(PeerResponse))
	        {
				foreach(var i in typeof(FairPpcClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
				{
					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }

	        if(ti.Type == typeof(NetException))
	        {
				foreach(var i in typeof(ProductException).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(NetException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Exception".Length))))
				{
					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }
	
	        return ti;
	    }
	}

	public class FairApiServer : McvApiServer
	{
		FairNode Node;

		public FairApiServer(FairNode node, ApiSettings settings, Flow workflow) : base(node, settings, workflow, FairApiClient.CreateOptions(node.Net))
		{
			Node = node;
		}

		protected override Type Create(string call)
		{
			return Type.GetType(typeof(FairApc).Namespace + '.' + call) ?? base.Create(call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
		{
			if(call is FairApc a)
				return a.Execute(Node, request, response, flow);
			else
				return base.Execute(call, request, response, flow);
		}
	}

	public class FairApiClient : McvApiClient
	{
		new public static JsonSerializerOptions CreateOptions(Net.Net net)
		{
			var o = McvApiClient.CreateOptions(net);

			o.TypeInfoResolver = new FairTypeResolver();
			
			return o;
		}

		public FairApiClient(HttpClient http, McvNet net, string address, string accesskey) : base(http, net, address, accesskey)
		{
			Options = CreateOptions(net);
			Net = net;
			
		}

		public FairApiClient(McvNet net, string address, string accesskey, int timeout = 30) : base(net, address, accesskey, timeout)
		{
			Options = CreateOptions(net);
			Net = net;
		}
	}


#if ETHEREUM
	public class EstimateEmitApc : FairApc
	{
		public byte[]			FromPrivateKey { get; set; } 
		public BigInteger		Wei { get; set; } 

		public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return rdn.Ethereum.EstimateEmission(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Net as FairNet).EthereumNetwork)), Wei, workflow);
		}
	}

	public class EmitApc : FairApc
	{
		public byte[]				FromPrivateKey { get; set; } 
		public AccountAddress		To { get; set; } 
		public int					Eid { get; set; } 
		public BigInteger			Wei { get; set; } 
		public BigInteger			Gas { get; set; } 
		public BigInteger			GasPrice { get; set; } 

		public class Response
		{
			
		}

		public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return rdn.Ethereum.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Net as FairNet).EthereumNetwork)), To, Wei, Eid, Gas, GasPrice, workflow);
			//return sun.Enqueue(o, sun.Vault.GetKey(To), Await, workflow);
		}
	}

	public class EmissionApc : FairApc
	{
		public AccountAddress		By { get; set; } 
		public int					Eid { get; set; } 
		public TransactionStatus	Await { get; set; }

		public override object Execute(FairNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var o = sun.Ethereum.FindEmission(By, Eid, workflow);

			return o;
		}
	}
#endif

	public class CostApc : FairApc
	{
		public class Return
		{
			//public Money		RentBytePerDay { get; set; }
			//public Money		Exeunit { get; set; }

			public Unit		RentAccount { get; set; }

			public Unit[][]	RentPublisher { get; set; }
			
			public Unit[]	RentProduct { get; set; }

			public Unit[]	RentProductData { get; set; }
			public Unit		RentProductDataForever { get; set; }
		}

		public Unit		Rate { get; set; } = 1;
		public byte[]	Years { get; set; }
		public byte[]	PublisherLengths { get; set; }

		public override object Execute(FairNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			if(Rate == 0)
			{
				Rate = 1;
			}

			var r = rdn.Peering.Call(() => new CostRequest(), workflow);

			return new Return {	//RentBytePerDay				= r.RentPerBytePerDay * Rate,
								//Exeunit						= r.ConsensusExeunitFee * Rate,
				
								RentAccount					= FairOperation.SpacetimeFee(Mcv.EntityLength, Mcv.Forever) * Rate,
					
								//RentPublisher				= Years.Select(y => PublisherLengths.Select(l => FairOperation.NameFee(y, new string(' ', l)) * Rate).ToArray()).ToArray(),
					
								RentProduct					= Years.Select(y => FairOperation.SpacetimeFee(Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
				
								RentProductData				= Years.Select(y => FairOperation.SpacetimeFee(1, Time.FromYears(y)) * Rate).ToArray(),
								RentProductDataForever		= FairOperation.SpacetimeFee(1, Mcv.Forever) * Rate};
		}
	}
}
