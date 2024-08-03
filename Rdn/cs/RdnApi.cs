using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Rdn
{
	public abstract class RdnApc : McvApc
	{
		public abstract object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

		public override object Execute(McvNode mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return Execute(mcv as RdnNode, request, response, workflow);
		}
	}

	public class RdnTypeResolver : ApiTypeResolver
	{
	    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
	    {
	        var ti = base.GetTypeInfo(type, options);

	        if(ti.Type == typeof(PeerRequest))
	        {
				foreach(var i in typeof(RdnPeerCallClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerRequest)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Request".Length))))
				{
					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }

	        if(ti.Type == typeof(PeerResponse))
	        {
				foreach(var i in typeof(RdnPeerCallClass).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(PeerResponse)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Remove(i.Name.Length - "Response".Length))))
				{
					ti.PolymorphismOptions.DerivedTypes.Add(i);
				}
	        }
	
	        return ti;
	    }
	}

	public class RdnApiServer : McvApiServer
	{
		RdnNode Node;

		public RdnApiServer(RdnNode node, Flow workflow) : base(node, workflow, RdnApiClient.DefaultOptions)
		{
			Node = node;
		}

		protected override Type Create(string call)
		{
			return Type.GetType(typeof(RdnApc).Namespace + '.' + call) ?? base.Create(call);
		}

		protected override object Execute(object call, HttpListenerRequest request, HttpListenerResponse response, Flow flow)
		{
			if(call is RdnApc a)
				return a.Execute(Node, request, response, flow);
			else
				return base.Execute(call, request, response, flow);
		}
	}

	public class RdnApiClient : McvApiClient
	{
		new public static readonly JsonSerializerOptions DefaultOptions;
		
		static RdnApiClient()
		{
			DefaultOptions = new JsonSerializerOptions{};
			DefaultOptions.IgnoreReadOnlyProperties = true;
			DefaultOptions.TypeInfoResolver = new RdnTypeResolver();

			foreach(var i in McvApiClient.DefaultOptions.Converters)
			{
				DefaultOptions.Converters.Add(i);
			}

			DefaultOptions.Converters.Add(new UraJsonConverter());
			DefaultOptions.Converters.Add(new UrrJsonConverter());
			DefaultOptions.Converters.Add(new ResourceDataJsonConverter());
		}

		public RdnApiClient(HttpClient http, string address, string accesskey) : base(http, address, accesskey)
		{
			Options = DefaultOptions;
		}

		public RdnApiClient(string address, string accesskey, int timeout = 30) : base(address, accesskey, timeout)
		{
			Options = DefaultOptions;
		}

		public LocalReleaseApc.Return Download(Ura address, Flow flow)
		{
  			var r = Request<Resource>(new ResourceDownloadApc {Identifier = new (address)}, flow);

			do
			{
				var d = Request<ResourceActivityProgress>(new LocalReleaseActivityProgressApc {Release = r.Data.Parse<Urr>()}, flow);
			
				if(d is null)
				{
					return Request<LocalReleaseApc.Return>(new LocalReleaseApc {Address = r.Data.Parse<Urr>()}, flow);
						
					//if(lrr.Availability == Availability.Full)
					//{
					//	return lrr;
					//}
 					//else
 					//{
 					//	throw new ResourceException(ResourceError.);
 					//}
				}
	
				Thread.Sleep(100);
			}
			while(flow.Active);

			throw new OperationCanceledException();
		}
	}

	public class GetApc : RdnApc
	{
		public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			try
			{
				var a = Ura.Parse(request.QueryString["address"]);
				var path = request.QueryString["path"] ?? "";
	
				var r = rdn.Call(() => new ResourceRequest(a), workflow).Resource;
				var ra = r.Data?.Parse<Urr>()
						 ??	
						 throw new ResourceException(ResourceError.NotFound);
	
				LocalResource s;
				LocalRelease z;
	
				lock(rdn.ResourceHub.Lock)
				{
					s = rdn.ResourceHub.Find(a) ?? rdn.ResourceHub.Add(a);
					z = rdn.ResourceHub.Find(ra) ?? rdn.ResourceHub.Add(ra);
				}
	
				IIntegrity itg = null;
	
				switch(ra)
				{ 
					case Urrh x :
						if(r.Data.Type.Control == DataType.File)
						{
							itg = new DHIntegrity(x.Hash); 
						}
						else if(r.Data.Type.Control == DataType.Directory)
						{
							var	f = rdn.ResourceHub.GetFile(z, false, LocalRelease.Index, null, new DHIntegrity(x.Hash), null, workflow);
	
							var index = new Xon(f.Read());
	
							itg = new DHIntegrity(index.Get<byte[]>(path)); 
						}
						break;
	
					case Urrsd x :
						var d = rdn.Call(() => new DomainRequest(a.Domain), workflow).Domain;
						var aa = rdn.Call(() => new AccountRequest(d.Owner), workflow).Account;
						itg = new SPDIntegrity(rdn.Zone.Cryptography, x, aa.Address);
						break;
	
					default:
						throw new ResourceException(ResourceError.NotSupportedDataType);
				}
	
				response.ContentType = MimeTypes.MimeTypeMap.GetMimeType(path);

				if(!z.IsReady(path))
				{
					FileDownload d;
	
					lock(rdn.ResourceHub.Lock)
						d = rdn.ResourceHub.DownloadFile(z, true, path, null, itg, null, workflow);
		
					var ps = new List<FileDownload.Piece>();
					int last = -1;
		
					d.PieceSucceeded += p => {
												if(!ps.Any())
													response.ContentLength64 = d.Length;
														
												ps.Add(p);
		
												while(workflow.Active)
												{
													var i = ps.FirstOrDefault(i => i.I - 1 == last);
		
													if(i != null)
													{	
														response.OutputStream.Write(i.Data.ToArray(), 0, (int)i.Data.Length);
														last = i.I;
													}
													else
														break;;
												}
											};
	
					d.Task.Wait(workflow.Cancellation);
				}
				else
				{
					lock(rdn.ResourceHub.Lock)
					{
						response.ContentLength64 = z.Find(path).Length;
						response.OutputStream.Write(z.Find(path).Read());
					}
				}
			}
			catch(EntityException ex) when(ex.Error == EntityError.NotFound)
			{
				response.StatusCode = (int)HttpStatusCode.NotFound;
			}
	
			return null;
		}
	}

#if ETHEREUM
	public class EstimateEmitApc : RdnApc
	{
		public byte[]			FromPrivateKey { get; set; } 
		public BigInteger		Wei { get; set; } 

		public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return rdn.Ethereum.EstimateEmission(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Zone as RdnZone).EthereumNetwork)), Wei, workflow);
		}
	}

	public class EmitApc : RdnApc
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

		public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return rdn.Ethereum.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Zone as RdnZone).EthereumNetwork)), To, Wei, Eid, Gas, GasPrice, workflow);
			//return sun.Enqueue(o, sun.Vault.GetKey(To), Await, workflow);
		}
	}

	public class EmissionApc : RdnApc
	{
		public AccountAddress		By { get; set; } 
		public int					Eid { get; set; } 
		public TransactionStatus	Await { get; set; }

		public override object Execute(RdnNode sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var o = sun.Ethereum.FindEmission(By, Eid, workflow);

			return o;
		}
	}
#endif

	public class CostApc : RdnApc
	{
		public class Return
		{
			//public Money		RentBytePerDay { get; set; }
			//public Money		Exeunit { get; set; }

			public Unit		RentAccount { get; set; }

			public Unit[][]	RentDomain { get; set; }
			
			public Unit[]		RentResource { get; set; }
			public Unit		RentResourceForever { get; set; }

			public Unit[]		RentResourceData { get; set; }
			public Unit		RentResourceDataForever { get; set; }
		}

		public Unit	Rate { get; set; } = 1;
		public byte[]	Years { get; set; }
		public byte[]	DomainLengths { get; set; }

		public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			if(Rate == 0)
			{
				Rate = 1;
			}

			var r = rdn.Call(() => new CostRequest(), workflow);

			return new Return {	//RentBytePerDay				= r.RentPerBytePerDay * Rate,
								//Exeunit						= r.ConsensusExeunitFee * Rate,
				
								RentAccount					= RdnOperation.SpacetimeFee(Mcv.EntityLength, Mcv.Forever) * Rate,
					
								RentDomain					= Years.Select(y => DomainLengths.Select(l => RdnOperation.NameFee(y, new string(' ', l)) * Rate).ToArray()).ToArray(),
					
								RentResource				= Years.Select(y => RdnOperation.SpacetimeFee(Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceForever			= RdnOperation.SpacetimeFee(Mcv.EntityLength, Mcv.Forever) * Rate,
				
								RentResourceData			= Years.Select(y => RdnOperation.SpacetimeFee(1, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceDataForever		= RdnOperation.SpacetimeFee(1, Mcv.Forever) * Rate};
		}
	}
}
