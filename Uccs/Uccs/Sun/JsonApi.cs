using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public abstract class ApiCall
	{
		public string			ProtocolVersion { get; set; }
		public string			AccessKey { get; set; }

		public static string	NameOf<C>() => NameOf(typeof(C));
		public static string	NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));

		public abstract object	Execute(Sun sun, Workflow workflow);
	}

	public class BatchCall : ApiCall
	{
		public class Item
		{
			public string Name { get; set; }
			public dynamic Call { get; set; }
		}

		public IEnumerable<Item> Calls { get; set; }

		public void Add(ApiCall call)
		{
			if(Calls == null)
				Calls = new List<Item>();

			(Calls as List<Item>).Add(new Item {Name = call.GetType().Name.Remove(call.GetType().Name.IndexOf("Call")), Call = call});
		}

		public override object Execute(Sun sun, Workflow workflow)
		{
			return null;
		}
	}

	public class ExitCall : ApiCall
	{
		public string Reason { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.Stop("Json API Call");
			return null;
		}
	}

	public class SettingsCall : ApiCall
	{
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return new SettingsResponse {	ProfilePath	= sun.Settings.Profile, 
												Settings	= sun.Settings}; /// TODO: serialize
		}
	}

	public class SettingsResponse
	{
		public string		ProfilePath {get; set;}
		public Settings		Settings {get; set;}
	}

	public class LogReportCall : ApiCall
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return new LogResponse{Log = sun.Workflow.Log.Messages.TakeLast(Limit).Select(i => i.ToString()).ToArray() }; 
		}
	}

	public class LogResponse
	{
		public IEnumerable<string> Log { get; set; }
	}

	public class PeersReportCall : ApiCall
	{
		public int		Limit { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
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

	public class SummaryReportCall : ApiCall
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return new SummaryResponse{Summary = sun.Summary.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
		}
	}

	public class SummaryResponse
	{
		public IEnumerable<string[]> Summary {get; set;}
	}

	public class ChainReportCall : ApiCall
	{
		public int		Limit  { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return new ChainReportResponse{Rounds = sun.Mcv.Tail.Take(Limit)
																	.Reverse()
																	.Select(i => new ChainReportResponse.Round
																				{
																					Id = i.Id, 
																					Members = i.Members.Count,
																					Analyzers = i.Analyzers.Count,
																					Voted = i.Voted,
																					Confirmed = i.Confirmed,
																					Time = i.ConfirmedTime,
																					Hash = i.Hash,
																					Summary = i.Summary,
																					Votes = i.Votes.Select(b => new ChainReportResponse.Vote {	Generator = b.Generator, 
																																				IsPayload = b.Transactions.Any(), 
																																					/*Confirmed = i.Confirmed && i.Transactions.Any() && i.ConfirmedPayloads.Contains(b)*/ }),
																					JoinRequests = i.JoinRequests.Select(i => i.Generator),
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
			public int							Analyzers {get; set;}
			public bool							Voted {get; set;}
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

	public class VotesReportCall : ApiCall
	{
		public int		RoundId  { get; set; }
		public int		Limit  { get; set; }


		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return new VotesReportResponse{Votes = sun.Mcv.FindRound(RoundId)?.Votes
																.OrderBy(i => i.Generator)
																.Take(Limit)
																.Select(i => new VotesReportResponse.Vote
																{
																	Try = i.Try,
																	Signature = i.Signature.ToHex(),
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
			public string			Signature { get; set; }
			public AccountAddress	Generator { get; set; }
		}

		public IEnumerable<Vote> Votes {get; set;}
	}

	public class RunNodeCall : ApiCall
	{
		public Role	Roles	{ get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.RunNode(null, Roles);
							
			if(Roles.HasFlag(Role.Seed))
				sun.RunSeed();
			
			return null;
		}
	}

	public class AddWalletCall : ApiCall
	{
		public byte[]	PrivateKey { get; set; }
		public string	Password { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.AddWallet(PrivateKey, Password);
			
			return null;
		}
	}

	public class SaveWalletCall : ApiCall
	{
		public AccountAddress	Account { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.SaveWallet(Account);
			
			return null;
		}
	}

	public class UnlockWalletCall : ApiCall
	{
		public AccountAddress	Account { get; set; }
		public string			Password { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.Unlock(Account, Password);

			return null;
		}
	}

	public class SetGeneratorCall : ApiCall
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Settings.Generators = Generators.Select(i => sun.Vault.GetKey(i)).ToList();

			return null;
		}
	}

	public class OperationResponse
	{
		public Operation Operation {get; set;} 
	}

	public class EmitCall : ApiCall
	{
		public byte[]			FromPrivateKey { get; set; } 
		public BigInteger		Wei { get; set; } 
		public AccountAddress	To { get; set; } 
		public PlacingStage		Await { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var o = sun.Emit(new Nethereum.Web3.Accounts.Account(FromPrivateKey, new BigInteger((int)sun.Zone.EthereumNetwork)), Wei, sun.Vault.GetKey(To), Await, workflow);

			return sun.Enqueue(o, sun.Vault.GetKey(To), Await, workflow);
		}
	}

	public class EnqeueOperationCall : ApiCall
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }
		public PlacingStage				Await  { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			return sun.Enqueue(Operations, sun.Vault.GetKey(By), Await, workflow);
		}
	}

	public class RdcCall : ApiCall
	{
		public RdcRequest	Request { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			return sun.Call(i => i.Request(Request), workflow);
		}
	}

	public class ResourceQueryCall : ApiCall
	{
		public string		Query { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return sun.QueryResource(Query).Resources;
		}
	}

	public class PackageAddCall : ApiCall
	{
		public PackageAddress	Package { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var m = new Manifest();
			m.Read(new BinaryReader(new MemoryStream(Manifest)));
								
			var h = sun.Zone.Cryptography.HashFile(m.Bytes);
								
			lock(sun.ResourceHub.Lock)
			{
				var r = sun.ResourceHub.Add(Package, ResourceType.Package, h);
	
				r.AddFile(Net.Package.ManifestFile, Manifest);
	
				if(Complete != null)
					r.AddFile(Net.Package.CompleteFile, Complete);

				if(Incremental != null)
					r.AddFile(Net.Package.IncrementalFile, Incremental);
								
				r.Complete((Complete != null ? Availability.Complete : 0) | (Incremental != null ? Availability.Incremental : 0));
				sun.ResourceHub.SetLatest(Package, h);
			}

			return null;
		}
	}

	public class ResourceBuildCall : ApiCall
	{
		public ResourceAddress		Resource { get; set; }
		public IEnumerable<string>	Sources { get; set; }
		public string				FilePath { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{
				if(FilePath != null)
					return sun.ResourceHub.Build(Resource, FilePath, workflow).Hash;
				else if(Sources != null && Sources.Any())
					return sun.ResourceHub.Build(Resource, Sources, workflow).Hash;
			}

			return null;
		}
	}

	public class ResourceDownloadCall : ApiCall
	{
		public ResourceAddress Resource { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			var r = sun.Call(c => c.FindResource(Resource), workflow).Resource;
					
			Release rs;

			lock(sun.ResourceHub.Lock)
			{
				rs = sun.ResourceHub.Find(Resource, r.Data) ?? sun.ResourceHub.Add(Resource, r.Type, r.Data);
	
				if(r.Type == ResourceType.File)
				{
					sun.ResourceHub.DownloadFile(rs, "f", r.Data, null, workflow);
					
					return r.Data;
				}
				else if(r.Type == ResourceType.Directory)
				{
					sun.ResourceHub.DownloadDirectory(rs, workflow);
					
					return r.Data;
				}
			}
	
			throw new NotSupportedException();
		}
	}
	
	public class ResourceDownloadProgressCall : ApiCall
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
				return sun.ResourceHub.GetDownloadProgress(Resource, Hash);
		}
	}
	
	public class ResourceInfoCall : ApiCall
	{
		public ResourceAddress	Resource { get; set; }
		public byte[]			Hash { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.ResourceHub.Lock)
			{	
				var r = sun.Call<ResourceResponse>(p => p.FindResource(Resource), workflow);

				var a = sun.ResourceHub.Find(Resource, Hash);

				return new ResourceInfo{ LocalAvailability	= a != null ? a.Availability : Availability.None,
										 LocalLatest		= a != null ? a.Hash : null,
										 LocalLatestFiles	= a != null ? a.Files.Count() : 0,
										 Entity				= r.Resource };
			}
		}
	}

	public class ResourceInfo
	{
		public Availability		LocalAvailability { get; set; }
		public byte[]			LocalLatest { get; set; }
		public int				LocalLatestFiles { get; set; }
		public Resource			Entity { get; set; }
	}

	public class PackageBuildCall : ApiCall
	{
		public PackageAddress		Package { get; set; }
		public Version				Version { get; set; }
		public IEnumerable<string>	Sources { get; set; }
		public string				DependsDirectory { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.AddRelease(Package, Version, Sources, DependsDirectory, workflow);

			return null;
		}
	}

	public class PackageDownloadCall : ApiCall
	{
		public PackageAddress		Package { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.Download(Package, workflow);

			return null;
		}
	}

	public class PackageDownloadProgressCall : ApiCall
	{
		public PackageAddress	Package { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				return sun.PackageHub.GetDownloadProgress(Package);
		}
	}

	public class PackageInfoCall : ApiCall
	{
		public PackageAddress	Package { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
			{
				var p = sun.PackageHub.Find(Package);

				if(p == null)
					throw new RdcEntityException(RdcEntityError.NotFound);

				return new PackageInfo{ Ready			= sun.PackageHub.IsReady(Package),
										Availability	= p.Release.Availability,
										Manifest		= p.Manifest };
			}
		}
	}

	public class PackageInfo
	{
		public bool				Ready { get; set; }
		public Availability		Availability { get; set; }
		public Manifest			Manifest { get; set; }
	}

	public class PackageInstallCall : ApiCall
	{
		public PackageAddress	Package { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.Install(Package, workflow);

			return null;
		}
	}

	public class GenerateAnalysisReportCall : ApiCall
	{
		public IDictionary<ResourceAddress, AnalysisResult>	Results { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
			{	
				sun.Analyses.AddRange(Results.Select(i => new Analysis {Resource = i.Key, Result = i.Value}));
			}

			return null;
		}
	}
}
