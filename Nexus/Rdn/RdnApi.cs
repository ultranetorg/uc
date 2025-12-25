using System.Net;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Rdn;

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
			foreach(var i in typeof(Rdn).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Ppc".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        if(ti.Type == typeof(Result))
			foreach(var i in typeof(Rdn).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(ti.Type) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Ppr".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

        if(ti.Type == typeof(CodeException))
			foreach(var i in typeof(Rdn).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(CodeException)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name.Substring(0, i.Name.Length - "Exception".Length))))
				ti.PolymorphismOptions.DerivedTypes.Add(i);

         if(ti.Type == typeof(Operation))
 			foreach(var i in typeof(RdnOperation).Assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(RdnOperation)) && !i.IsAbstract && !i.IsGenericType).Select(i => new JsonDerivedType(i, i.Name)))
 				ti.PolymorphismOptions.DerivedTypes.Add(i);

        return ti;
    }
}

public class RdnApiServer : McvApiServer
{
	RdnNode Node;

	public RdnApiServer(RdnNode node, ApiSettings settings, Flow workflow) : base(node, settings, workflow, RdnApiClient.CreateOptions())
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
	public LocalResource	FindLocalResource(Ura address, Flow flow) => Call<LocalResource>(new LocalResourceApc {Address = address}, flow);
	public LocalReleaseApe	FindLocalRelease(Urr address, Flow flow) => Call<LocalReleaseApe>(new LocalReleaseApc {Address = address}, flow);

	new public static JsonSerializerOptions CreateOptions()
	{
		var o = McvApiClient.CreateOptions();

		o.TypeInfoResolver = new RdnTypeResolver();

		o.Converters.Add(new UraJsonConverter());
		o.Converters.Add(new UrrJsonConverter());
		o.Converters.Add(new ResourceDataJsonConverter());

		return o;
	}

	public RdnApiClient(string address, string accesskey, HttpClient http = null, int timeout = 30) : base(address, accesskey, http, timeout)
	{
		Options = CreateOptions();
	}

	public LocalReleaseApe Download(Ura address, Flow flow)
	{
		var r = Call<Resource>(new ResourceDownloadApc {Identifier = new(address)}, flow);

		do
		{
			var d = Call<ResourceActivityProgress>(new LocalReleaseActivityProgressApc {Release = r.Data.Parse<Urr>()}, flow);

			if(d is null)
			{
				return Call<LocalReleaseApe>(new LocalReleaseApc {Address = r.Data.Parse<Urr>()}, flow);

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

public class HttpGetApc : RdnApc
{
	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		try
		{
			var a = Ura.Parse(request.QueryString["address"]);
			var path = request.QueryString["path"] ?? "";

			var r = rdn.Peering.Call(new ResourcePpc(a), workflow).Resource;
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
					var d = rdn.Peering.Call(new DomainPpc(a.Domain), workflow).Domain;
					var aa = rdn.Peering.Call(new UserPpc(d.Owner), workflow).User;
					itg = new SPDIntegrity(rdn.Net.Cryptography, x, aa.Owner);
					break;

				default:
					throw new ResourceException(ResourceError.NotSupportedDataType);
			}

			response.ContentType = MimeTypeMap.GetMimeType(path);

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
		return rdn.Ethereum.EstimateEmission(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Net as RdnNet).EthereumNetwork)), Wei, workflow);
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
		return rdn.Ethereum.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)(rdn.Net as RdnNet).EthereumNetwork)), To, Wei, Eid, Gas, GasPrice, workflow);
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
		
		public Unit[]	RentResource { get; set; }
		public Unit		RentResourceForever { get; set; }

		public Unit[]	RentResourceData { get; set; }
		public Unit		RentResourceDataForever { get; set; }
	}

	public Unit		Rate { get; set; } = 1;
	public byte[]	Years { get; set; }
	public byte[]	DomainLengths { get; set; }

	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(Rate == 0)
		{
			Rate = 1;
		}

		var r = rdn.Peering.Call(new CostPpc(), workflow);
	
		return	new Return
				{	//RentBytePerDay				= r.RentPerBytePerDay * Rate,
					//Exeunit						= r.ConsensusExeunitFee * Rate,
				
					RentAccount					= Execution.ToBD(rdn.Net.EntityLength, Mcv.Forever) * Rate,
					
					RentDomain					= Years.Select(y => DomainLengths.Select(l => RdnExecution.NameFee(y, new string(' ', l)) * Rate).ToArray()).ToArray(),
					
					RentResource				= Years.Select(y => Execution.ToBD(rdn.Net.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
					RentResourceForever			= Execution.ToBD(rdn.Net.EntityLength, Mcv.Forever) * Rate,
				
					RentResourceData			= Years.Select(y => Execution.ToBD(1, Time.FromYears(y)) * Rate).ToArray(),
					RentResourceDataForever		= Execution.ToBD(1, Mcv.Forever) * Rate
				};
	}
}

public class NnHolderClassesApc : RdnApc
{
	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(rdn.Mcv.Lock)
		{	
			return new string[] {nameof(User)};
		}
	}
}

public class NnHoldersByAccountApc : RdnApc
{
	public byte[]	Address { get; set; }

	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		lock(rdn.Mcv.Lock)
		{	
			throw new NotImplementedException();

			var a = rdn.Mcv.Users.Latest(null);
			
			if(a != null)
				return new string[] {EntityAddress.ToString(McvTable.User, a.Id)};
			else
				throw new NnpException(NnpError.NotFound);
		}
	}
}

public class NnHolderAssetsApc : RdnApc
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }

	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(HolderClass != nameof(User))
			throw new NnpException(NnpError.Unknown);

		lock(rdn.Mcv.Lock)
		{	
			var a = rdn.Mcv.Users.Latest(AutoId.Parse(HolderId));
			
			if(a != null)
				return new Asset[]	{
										new () {Name = nameof(User.Spacetime), Units = "Byte-days (BD)"},
										new () {Name = nameof(User.Energy), Units = "Execution Cycles (EC)"},
									};
			else
				throw new NnpException(NnpError.NotFound);
		}
	}
}

public class NnAssetBalanceApc : RdnApc
{
	public string	HolderClass { get; set; }
	public string	HolderId { get; set; }
	public string	Name { get; set; }

	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
		if(HolderClass != nameof(User))
			throw new NnpException(NnpError.Unknown);

		if(Name != nameof(User.Spacetime) && Name != nameof(User.Energy))
			throw new NnpException(NnpError.Unknown);

		lock(rdn.Mcv.Lock)
		{	
			var a = rdn.Mcv.Users.Latest(AutoId.Parse(HolderId));
			
			if(a != null)
				return new BigInteger (Name switch
											{
												nameof(User.Spacetime) => a.Spacetime,
												nameof(User.Energy) => a.Energy,
											});
			else
				throw new NnpException(NnpError.NotFound);
		}
	}
}

public class NnTransferApc : RdnApc
{
	public string	FromClass { get; set; }
	public string	FromId { get; set; }
	public string	Name { get; set; }
	public string	ToClass { get; set; }
	public string	ToId { get; set; }

	public override object Execute(RdnNode rdn, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
	{
//		if(HolderClass != nameof(Account))
//			throw new NnException(NnError.Unknown);
//
//		if(Name != nameof(Account.Spacetime) && Name != nameof(Account.Energy))
//			throw new NnException(NnError.Unknown);
//
//		lock(rdn.Mcv.Lock)
//		{	
//			var a = rdn.Mcv.Accounts.Latest(AutoId.Parse(HolderId));
//			
//			if(a != null)
//				return new BigInteger (Name switch
//											{
//												nameof(Account.Spacetime) => a.Spacetime,
//												nameof(Account.Energy) => a.Energy,
//											});
//			else
//				throw new NnException(NnError.NotFound);

//		}
		return null;
	}
}