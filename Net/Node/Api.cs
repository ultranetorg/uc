﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;

namespace Uccs.Net
{
	public abstract class SunApiCall : ApiCall
	{
		public abstract object	Execute(Sun sun, Workflow workflow);
	}

	public class PropertyCall : SunApiCall
	{
		public string Path { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
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

	public class ExceptionCall : SunApiCall
	{
		public string Reason { get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			throw new Exception("TEST");
		}
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
			{ 
				List<KeyValuePair<string, string>> f = new();
															
				f.Add(new ("Version",					sun.Version.ToString()));
				f.Add(new ("Zone",						sun.Zone.Name));
				f.Add(new ("Profile",					sun.Settings.Profile));
				f.Add(new ("IP(Reported):Port",			$"{sun.Settings.IP} ({sun.IP}) : {sun.Zone.Port}"));
				f.Add(new ("Incoming Transactions",		$"{sun.IncomingTransactions.Count}"));
				f.Add(new ("Outgoing Transactions",		$"{sun.OutgoingTransactions.Count}"));
				f.Add(new ("    Pending Delegation",	$"{sun.OutgoingTransactions.Count(i => i.Placing == PlacingStage.Pending)}"));
				f.Add(new ("    Accepted",				$"{sun.OutgoingTransactions.Count(i => i.Placing == PlacingStage.Accepted)}"));
				//f.Add(new ("    Pending Placement",		$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Verified)}"));
				f.Add(new ("    Placed",				$"{sun.OutgoingTransactions.Count(i => i.Placing == PlacingStage.Placed)}"));
				f.Add(new ("    Confirmed",				$"{sun.OutgoingTransactions.Count(i => i.Placing == PlacingStage.Confirmed)}"));
				f.Add(new ("Votes Acceped/Rejected",	$"{sun.Statistics.AccpetedVotes}/{sun.Statistics.RejectedVotes}"));
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

	public class RunNodeCall : SunApiCall
	{
		public Role	Roles	{ get; set; }

		public override object Execute(Sun sun, Workflow workflow)
		{
			sun.RunNode(Roles);
							
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

	public class SetGeneratorCall : SunApiCall
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}

		public override object Execute(Sun sun, Workflow workflow)
		{
			lock(sun.Lock)
				sun.Settings.Generators = Generators.ToList();

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
		public PlacingStage				Await  { get; set; } = PlacingStage.Confirmed;

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