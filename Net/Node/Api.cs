using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;

namespace Uccs.Net
{
	public abstract class SunApc : Apc
	{
		public abstract object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow);
	}

	public class GetApc : SunApc
	{
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			try
			{
				var a = Ura.Parse(request.QueryString["address"]);
				var path = request.QueryString["path"] ?? "f";
	
				var r = sun.Call(p => p.Request(new ResourceByNameRequest {Name = a}), workflow).Resource;
				var ra = r.Data?.Interpretation as Urr
						 ??	
						 throw new ResourceException(ResourceError.NotFound);
	
				LocalResource s;
				LocalRelease z;
	
				lock(sun.ResourceHub.Lock)
				{
					s = sun.ResourceHub.Find(a) ?? sun.ResourceHub.Add(a);
					z = sun.ResourceHub.Find(ra) ?? sun.ResourceHub.Add(ra, r.Data.Type);
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
							var	f = sun.ResourceHub.GetFile(z, DirectoryDownload.Index, new DHIntegrity(x.Hash), null, workflow);
	
							var index = new XonDocument(f.Read());
	
							itg = new DHIntegrity(index.Get<byte[]>(path)); 
						}
						break;
	
					case Urrsd x :
						var au = sun.Call(c => c.Request(new DomainRequest {Name = a.Domain}), workflow).Domain;
						itg = new SPDIntegrity(sun.Zone.Cryptography, x, au.Owner);
						break;
	
					default:
						throw new ResourceException(ResourceError.NotSupportedDataType);
				}
	
				response.ContentType = MimeTypes.MimeTypeMap.GetMimeType(path);

				if(!z.IsReady(path))
				{
					FileDownload d;
	
					lock(sun.ResourceHub.Lock)
						d = sun.ResourceHub.DownloadFile(z, path, itg, null, workflow);
		
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
					lock(sun.ResourceHub.Lock)
					{
						response.ContentLength64 = z.GetLength(path);
						response.OutputStream.Write(z.ReadFile(path));
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

	public class PropertyApc : SunApc
	{
		public string Path { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			object o = sun;

			foreach(var i in Path.Split('.'))
			{
				o = o.GetType().GetProperty(i)?.GetValue(o) ?? o.GetType().GetField(i)?.GetValue(o);
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

	public class ExceptionApc : SunApc
	{
		public string Reason { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			throw new Exception("TEST");
		}
	}

	public class ExitApc : SunApc
	{
		public string Reason { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			sun.Stop("Json API Call");
			return null;
		}
	}

	public class SettingsApc : SunApc
	{
		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				return new SettingsResponse{ProfilePath	= sun.Settings.Profile, 
											Settings	= sun.Settings}; /// TODO: serialize
		}
	}

	public class SettingsResponse
	{
		public string		ProfilePath {get; set;}
		public Settings		Settings {get; set;}
	}

	public class LogReportApc : SunApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				return new LogResponse{Log = sun.Workflow.Log.Messages.TakeLast(Limit).Select(i => i.ToString()).ToArray() }; 
		}
	}

	public class LogResponse
	{
		public IEnumerable<string> Log { get; set; }
	}

	public class PeersReportApc : SunApc
	{
		public int		Limit { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				return new PeersReport{Peers = sun.Peers.Where(i => i.Status == ConnectionStatus.OK).TakeLast(Limit).Select(i =>	new PeersReport.Peer
																																	{
																																		IP			= i.IP,			
																																		Status		= i.StatusDescription,
																																		PeerRank	= i.PeerRank,
																																		ChainRank	= i.ChainRank,
																																		BaseRank	= i.BaseRank,
																																		SeedRank	= i.SeedRank,
																																		LastSeen	= i.LastSeen,
																																		LastTry		= i.LastTry,
																																		Retries		= i.Retries	
																																	}).ToArray()}; 
		}
	}

	public class PeersReport
	{
		public class Peer
		{
			public IPAddress	IP { get; set; }
			public string		Status  { get; set; }
			public int			PeerRank { get; set; }
			public int			ChainRank { get; set; }
			public int			BaseRank { get; set; }
			public int			SeedRank { get; set; }
			public DateTime		LastSeen { get; set; }
			public DateTime		LastTry { get; set; }
			public int			Retries { get; set; }
		}

		public IEnumerable<Peer> Peers {get; set;}
	}

	public class SummaryReportApc : SunApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
			{ 
				List<KeyValuePair<string, string>> f =
				[
					new ("Version",						sun.Version.ToString()),
					new ("Zone",						sun.Zone.Name),
					new ("Profile",						sun.Settings.Profile),
					new ("IP(Reported):Port",			$"{sun.Settings.IP} ({sun.IP}) : {sun.Zone.Port}"),
					new ("Incoming Transactions",		$"{sun.IncomingTransactions.Count}"),
					new ("Outgoing Transactions",		$"{sun.OutgoingTransactions.Count}"),
					new ("    Pending Delegation",		$"{sun.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Pending)}"),
					new ("    Accepted",				$"{sun.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Accepted)}"),
				//	new ("    Pending Placement",	$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Verified)}"));
					new ("    Placed",					$"{sun.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Placed)}"),
					new ("    Confirmed",				$"{sun.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Confirmed)}"),
					new ("Votes Acceped/Rejected",		$"{sun.Statistics.AccpetedVotes}/{sun.Statistics.RejectedVotes}"),
				];
				//f.Add(new ("Peers in/out/min/known",	$"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}"));
				
				if(sun.Mcv != null)
				{
					f.Add(new ("Synchronization",		$"{sun.Synchronization}"));
					f.Add(new ("Size",					$"{sun.Mcv.Size}"));
					f.Add(new ("Members",				$"{sun.Mcv.LastConfirmedRound?.Members.Count}"));
					f.Add(new ("Emission",				$"{sun.Mcv.LastConfirmedRound?.Emission.ToHumanString()}"));
					f.Add(new ("ExeunitMinFee",			$"{sun.Mcv.LastConfirmedRound?.ConsensusExeunitFee.ToHumanString()}"));
					f.Add(new ("SyncCache Blocks",		$"{sun.SyncTail.Sum(i => i.Value.Votes.Count)}"));
					f.Add(new ("Loaded Rounds",			$"{sun.Mcv.LoadedRounds.Count}"));
					f.Add(new ("Last Non-Empty Round",	$"{(sun.Mcv.LastNonEmptyRound != null ? sun.Mcv.LastNonEmptyRound.Id : null)}"));
					f.Add(new ("Last Payload Round",	$"{(sun.Mcv.LastPayloadRound != null ? sun.Mcv.LastPayloadRound.Id : null)}"));
					f.Add(new ("Base Hash",				sun.Mcv.BaseHash.ToHex()));
					f.Add(new ("Generating (nps/μs)",	$"{sun.Statistics.Generating	.N}/{sun.Statistics.Generating	.Avarage.Ticks/10}"));
					f.Add(new ("Consensing (nps/μs)",	$"{sun.Statistics.Consensing	.N}/{sun.Statistics.Consensing	.Avarage.Ticks/10}"));
					f.Add(new ("Transacting (nps/μs)",	$"{sun.Statistics.Transacting	.N}/{sun.Statistics.Transacting	.Avarage.Ticks/10}"));
					f.Add(new ("Declaring (nps/μs)",	$"{sun.Statistics.Declaring		.N}/{sun.Statistics.Declaring	.Avarage.Ticks/10}"));
					f.Add(new ("Sending (nps/μs)",		$"{sun.Statistics.Sending		.N}/{sun.Statistics.Sending		.Avarage.Ticks/10}"));
					f.Add(new ("Reading (nps/μs)",		$"{sun.Statistics.Reading		.N}/{sun.Statistics.Reading		.Avarage.Ticks/10}"));

					if(sun.Synchronization == Synchronization.Synchronized)
					{
						string formatbalance(AccountAddress a)
						{
							return sun.Mcv.Accounts.Find(a, sun.Mcv.LastConfirmedRound.Id)?.Balance.ToHumanString();
						}
	
						foreach(var i in sun.Vault.Wallets)
						{
							var a = i.Key.ToString();
							f.Add(new ($"{a.Substring(0, 8)}...{a.Substring(a.Length-8, 8)} {(sun.Vault.IsUnlocked(i.Key) ? "Unlocked" : "Locked")}", $"{formatbalance(i.Key),23}"));
						}
	
						if(SunGlobals.UI)
						{
						}
					}
				}
				else
				{
					//f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in sun.Vault.Wallets)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				sun.Statistics.Reset();
		
				return new SummaryResponse{Summary = f.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
			}
		}
	}

	public class SummaryResponse
	{
		public IEnumerable<string[]> Summary {get; set;}
	}

	public class ChainReportApc : SunApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				return new ChainReportResponse{Rounds = sun.Mcv.Tail.Take(Limit)
																	.Reverse()
																	.Select(i => new ChainReportResponse.Round
																				{
																					Id = i.Id, 
																					Members = i.Members == null ? 0 : i.Members.Count,
																					Confirmed = i.Confirmed,
																					Time = i.ConsensusTime,
																					Hash = i.Hash,
																					Votes = i.Votes.Select(b => new ChainReportResponse.Vote {	Generator = b.Generator, 
																																				IsPayload = b.Transactions.Any(), 
																																					/*Confirmed = i.Confirmed && i.Transactions.Any() && i.ConfirmedPayloads.Contains(b)*/ }),
																					JoinRequests = i.Transactions.SelectMany(i => i.Operations).OfType<CandidacyDeclaration>().Select(i => i.Transaction.Signer),
																				})
																	.ToArray()}; 
		}
	}

	public class ChainReportResponse
	{
		public class Vote
		{
			public AccountAddress	Generator {get; set;}
			public bool				IsPayload {get; set;}
			//public bool				Confirmed {get; set;}
		}

		public class Round
		{
			public int							Id {get; set;}
			public int							Members {get; set;}
			public bool							Confirmed {get; set;}
			public Time							Time {get; set;}
			public byte[]						Hash {get; set;}
			public byte[]						Summary {get; set;}
			public IEnumerable<Vote>			Votes {get; set;}
			public IEnumerable<AccountAddress>	JoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	HubJoinRequests {get; set;}
			//public IEnumerable<AccountAddress>	AnalyzerJoinRequests {get; set;}
		}

		public IEnumerable<Round> Rounds {get; set;}
	}

	public class VotesReportApc : SunApc
	{
		public int		RoundId  { get; set; }
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				return new VotesReportResponse{Votes = sun.Mcv.FindRound(RoundId)?.Votes
																.OrderBy(i => i.Generator)
																.Take(Limit)
																.Select(i => new VotesReportResponse.Vote
																{
																	Try = i.Try,
																	ParentSummary = i.ParentHash,
																	Signature = i.Signature,
																	Generator = i.Generator
																})
																.ToArray()}; 
		}
	}

	public class VotesReportResponse
	{
		public class Vote
		{
			public int				Try { get; set; }
			public byte[]			ParentSummary { get; set; }
			public byte[]			Signature { get; set; }
			public AccountAddress	Generator { get; set; }
		}

		public IEnumerable<Vote> Votes {get; set;}
	}

	public class RunNodeApc : SunApc
	{
		public Role	Roles	{ get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			sun.RunNode(Roles);
							
			if(Roles.HasFlag(Role.Seed))
				sun.RunSeed();
			
			return null;
		}
	}

	public class AddWalletApc : SunApc
	{
		public byte[]	Wallet { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.AddWallet(Wallet);
			
			return null;
		}
	}

	public class SaveWalletApc : SunApc
	{
		public AccountAddress	Account { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.SaveWallet(Account);
			
			return null;
		}
	}

	public class UnlockWalletApc : SunApc
	{
		public AccountAddress	Account { get; set; }
		public string			Password { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
			{
				if(Account != null)
					sun.Vault.Unlock(Account, Password);
				else
					foreach(var i in sun.Vault.Wallets)
						sun.Vault.Unlock(i.Key, Password);
			}

			return null;
		}
	}

	public class SetGeneratorApc : SunApc
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Settings.Generators = Generators.ToList();

			return null;
		}
	}

	public class EmitApc : SunApc
	{
		public byte[]			FromPrivateKey { get; set; } 
		public BigInteger		Wei { get; set; } 
		public AccountAddress	To { get; set; } 
		public TransactionStatus		Await { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			var o = sun.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)sun.Zone.EthereumNetwork)), Wei, sun.Vault.GetKey(To), Await, workflow);

			return sun.Enqueue(o, sun.Vault.GetKey(To), Await, workflow);
		}
	}

	public class EmissionApc : SunApc
	{
		public AccountAddress		By { get; set; } 
		public int					Eid { get; set; } 
		public TransactionStatus	Await { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			var o = sun.Nas.FindEmission(By, Eid, workflow);

			return o;
		}
	}

	public class CostApc : SunApc
	{
		public class Report
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

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			if(Rate == 0)
			{
				Rate = 1;
			}

			var r = sun.Call(i => i.Request(new CostRequest()), workflow);

			return new Report {	RentBytePerDay				= r.RentPerBytePerDay * Rate,
								Exeunit						= r.ConsensusExeunitFee * Rate,
				
								RentAccount					= Operation.CalculateFee(r.RentPerBytePerDay, Mcv.EntityLength, Mcv.Forever) * Rate,
					
								RentDomain					= Years.Select(y => DomainLengths.Select(l => DomainRegistration.CalculateFee(Time.FromYears(y), r.RentPerBytePerDay, l) * Rate).ToArray()).ToArray(),
					
								RentResource				= Years.Select(y => Operation.CalculateFee(r.RentPerBytePerDay, Mcv.EntityLength, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceForever			= Operation.CalculateFee(r.RentPerBytePerDay, Mcv.EntityLength, Mcv.Forever) * Rate,
				
								RentResourceData			= Years.Select(y => Operation.CalculateFee(r.RentPerBytePerDay, 1, Time.FromYears(y)) * Rate).ToArray(),
								RentResourceDataForever		= Operation.CalculateFee(r.RentPerBytePerDay, 1, Mcv.Forever) * Rate};
		}
	}

	public class EnqeueOperationApc : SunApc
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }
		public TransactionStatus				Await  { get; set; } = TransactionStatus.Confirmed;

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			return sun.Transact(Operations, sun.Vault.GetKey(By), Await, workflow);
		}
	}

	public class EstimateOperationApc : SunApc
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			var t = new Transaction {Zone = sun.Zone, Operations = Operations.ToArray()};
			t.Sign(sun.Vault.GetKey(By), []);

			return sun.Call(p => p.Request(new AllocateTransactionRequest {Transaction = t}), workflow);
		}
	}

	public class RdcApc : SunApc
	{
		public RdcRequest Request { get; set; }

		public override object Execute(Sun sun, HttpListenerRequest request, HttpListenerResponse response, Workflow workflow)
		{
			try
			{
				return sun.Call(i => i.Request(Request), workflow);
			}
			catch(SunException ex)
			{
				var rp = RdcResponse.FromType(Request.Class);
				rp.Error = ex;
				
				return rp;
			}
		}
	}
}
