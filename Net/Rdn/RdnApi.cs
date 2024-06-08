using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;

namespace Uccs.Net
{
	public abstract class RdnApc : McvApc
	{
		public abstract object Execute(Rdn sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return Execute(mcv as Rdn, request, response, workflow);
		}
	}

	public class GetApc : RdnApc
	{
		public override object Execute(Rdn rds, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			try
			{
				var a = Ura.Parse(request.QueryString["address"]);
				var path = request.QueryString["path"] ?? "";
	
				var r = rds.Call(() => new ResourceRequest(a), workflow).Resource;
				var ra = r.Data?.Interpretation as Urr
						 ??	
						 throw new ResourceException(ResourceError.NotFound);
	
				LocalResource s;
				LocalRelease z;
	
				lock(rds.ResourceHub.Lock)
				{
					s = rds.ResourceHub.Find(a) ?? rds.ResourceHub.Add(a);
					z = rds.ResourceHub.Find(ra) ?? rds.ResourceHub.Add(ra, r.Data.Type);
				}
	
				IIntegrity itg = null;
	
				switch(ra)
				{ 
					case Urrh x :
						if(r.Data.Type == DataType.File)
						{
							itg = new DHIntegrity(x.Hash); 
						}
						else if(r.Data.Type == DataType.Directory)
						{
							var	f = rds.ResourceHub.GetFile(z, LocalRelease.Index, null, new DHIntegrity(x.Hash), null, workflow);
	
							var index = new XonDocument(f.Read());
	
							itg = new DHIntegrity(index.Get<byte[]>(path)); 
						}
						break;
	
					case Urrsd x :
						var au = rds.Call(() => new DomainRequest(a.Domain), workflow).Domain;
						itg = new SPDIntegrity(rds.Zone.Cryptography, x, au.Owner);
						break;
	
					default:
						throw new ResourceException(ResourceError.NotSupportedDataType);
				}
	
				response.ContentType = MimeTypes.MimeTypeMap.GetMimeType(path);

				if(!z.IsReady(path))
				{
					FileDownload d;
	
					lock(rds.ResourceHub.Lock)
						d = rds.ResourceHub.DownloadFile(z, path, null, itg, null, workflow);
		
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
					lock(rds.ResourceHub.Lock)
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

	public class EstimateEmitApc : RdnApc
	{
		public byte[]			FromPrivateKey { get; set; } 
		public BigInteger		Wei { get; set; } 

		public override object Execute(Rdn sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return sun.Ethereum.EstimateEmission(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)sun.Zone.EthereumNetwork)), Wei, workflow);
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

		public override object Execute(Rdn sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return sun.Ethereum.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)sun.Zone.EthereumNetwork)), To, Wei, Eid, Gas, GasPrice, workflow);
			//return sun.Enqueue(o, sun.Vault.GetKey(To), Await, workflow);
		}
	}

	public class EmissionApc : RdnApc
	{
		public AccountAddress		By { get; set; } 
		public int					Eid { get; set; } 
		public TransactionStatus	Await { get; set; }

		public override object Execute(Rdn sun, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var o = sun.Ethereum.FindEmission(By, Eid, workflow);

			return o;
		}
	}

	public class CostApc : RdnApc
	{
		public class Return
		{
			public Money		RentBytePerDay { get; set; }
			public Money		Exeunit { get; set; }

			public Money		RentAccount { get; set; }

			public Money[][]	RentDomain { get; set; }
			
			public Money[]		RentResource { get; set; }
			public Money		RentResourceForever { get; set; }

			public Money[]		RentResourceData { get; set; }
			public Money		RentResourceDataForever { get; set; }
		}

		public Money	Rate { get; set; } = 1;
		public byte[]	Years { get; set; }
		public byte[]	DomainLengths { get; set; }

		public override object Execute(Rdn rds, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			if(Rate == 0)
			{
				Rate = 1;
			}

			var r = rds.Call(() => new CostRequest(), workflow);

			return new Return {	RentBytePerDay				= r.RentPerBytePerDay * Rate,
								Exeunit						= r.ConsensusExeunitFee * Rate,
				
								RentAccount					= Operation.SpaceFee(r.RentPerBytePerDay, Mcv.EntityLength, Mcv.Forever) * Rate,
					
								RentDomain					= Years.Select(y => DomainLengths.Select(l => Operation.NameFee(y, r.RentPerBytePerDay, new string(' ', l)) * Rate).ToArray()).ToArray(),
					
								RentResource				= Years.Select(y => Operation.SpaceFee(r.RentPerBytePerDay, Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceForever			= Operation.SpaceFee(r.RentPerBytePerDay, Mcv.EntityLength, Mcv.Forever) * Rate,
				
								RentResourceData			= Years.Select(y => Operation.SpaceFee(r.RentPerBytePerDay, 1, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceDataForever		= Operation.SpaceFee(r.RentPerBytePerDay, 1, Mcv.Forever) * Rate};
		}
	}
}
