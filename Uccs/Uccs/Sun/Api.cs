using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Uccs.Net
{
	public abstract class SunApiCall : ApiCall
	{
		public abstract object	Execute(Sun sun, Workflow workflow);
	}

	public class ExitCall : SunApiCall
	{
		public string Reason { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.Stop("Json API Call");
			return null;
		}
	}

	public class SettingsCall : SunApiCall
	{
		public override object Execute(Sun sun, Workflow workflow)
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

	public class LogReportCall : SunApiCall
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

	public class PeersReportCall : SunApiCall
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

	public class SummaryReportCall : SunApiCall
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

	public class ChainReportCall : SunApiCall
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

	public class VotesReportCall : SunApiCall
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

	public class RunNodeCall : SunApiCall
	{
		public Role	Roles	{ get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.RunNode(new Workflow("Main by Api call", new Log()), Roles);
							
			if(Roles.HasFlag(Role.Seed))
				sun.RunSeed();
			
			return null;
		}
	}

	public class AddWalletCall : SunApiCall
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

	public class SaveWalletCall : SunApiCall
	{
		public AccountAddress	Account { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Vault.SaveWallet(Account);
			
			return null;
		}
	}

	public class UnlockWalletCall : SunApiCall
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

	public class SetGeneratorCall : SunApiCall
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

	public class EmitCall : SunApiCall
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

	public class EnqeueOperationCall : SunApiCall
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }
		public PlacingStage				Await  { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			return sun.Enqueue(Operations, sun.Vault.GetKey(By), Await, workflow);
		}
	}

	public class RdcCall : SunApiCall
	{
		public RdcRequest	Request { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			return sun.Call(i => i.Request(Request), workflow);
		}
	}

	public class GenerateAnalysisReportCall : SunApiCall
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
