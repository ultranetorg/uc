using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public abstract class ApiCall
	{
		public string			ProtocolVersion { get; set; }
		public string			AccessKey { get; set; }

		public static string NameOf<C>() => NameOf(typeof(C));
		public static string NameOf(Type type) => type.Name.Remove(type.Name.IndexOf("Call"));

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

	public class UntTransferCall : ApiCall
	{
		public AccountAddress	From { get; set; }
		public AccountAddress	To { get; set; }
		public Money			Amount { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			AccountKey k;
								
			lock(sun.Lock)
				k = sun.Vault.GetKey(From);

			sun.Enqueue(new UntTransfer(To, Amount), k, PlacingStage.Accepted, workflow);

			return null;
		}
	}

	public class QueryResourceCall : ApiCall
	{
		public string		Query { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				return sun.QueryResource(Query).Resources;
		}
	}

	public class AddReleaseCall : ApiCall
	{
		public PackageAddress	Release { get; set; }
		public byte[]			Complete { get; set; }
		public byte[]			Incremental { get; set; }
		public byte[]			Manifest { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
			{
				var m = new Manifest();
				m.Read(new BinaryReader(new MemoryStream(Manifest)));
								
				var h = sun.Zone.Cryptography.HashFile(m.Bytes);
								
				lock(sun.ResourceHub.Lock)
				{
					sun.ResourceHub.Add(Release, h);
	
					sun.ResourceHub.WriteFile(Release, h, Package.ManifestFile, 0, Manifest);
	
					if(Complete != null)
					{
						sun.ResourceHub.WriteFile(Release, h, Package.CompleteFile, 0, Complete);
					}
					if(Incremental != null)
					{
						sun.ResourceHub.WriteFile(Release, h, Package.IncrementalFile, 0, Incremental);
					}
								
					sun.ResourceHub.SetLatest(Release, h);
				}
			}

			return null;
		}
	}

	//public class DownloadReleaseCall : ApiCall
	//{
	//	public ReleaseAddress	Release { get; set; }
	//}

	public class PackageStatusCall : ApiCall
	{
		public PackageAddress	Release { get; set; }
		public int				Limit  { get; set; }
		
		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				return sun.PackageHub.GetStatus(Release, Limit);
		}
	}

	public class InstallPackageCall : ApiCall
	{
		public PackageAddress	Release { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.PackageHub.Lock)
				sun.PackageHub.Install(Release, workflow);

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
