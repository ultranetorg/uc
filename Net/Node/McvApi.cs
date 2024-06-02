using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Uccs.Net
{
	public abstract class McvApc : Apc
	{
		public Guid	 Mcvid { get; set; }

		public abstract object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow);
	}

	public class McvSummaryApc : McvApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Sun.Lock)
			{ 
				List<KeyValuePair<string, string>> f =
				[
					new ("Incoming Transactions",		$"{mcv.IncomingTransactions.Count}"),
					new ("Outgoing Transactions",		$"{mcv.OutgoingTransactions.Count}"),
					new ("    Pending Delegation",		$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Pending)}"),
					new ("    Accepted",				$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Accepted)}"),
				//	new ("    Pending Placement",	$"{OutgoingTransactions.Count(i => i.Placing == PlacingStage.Verified)}"));
					new ("    Placed",					$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Placed)}"),
					new ("    Confirmed",				$"{mcv.OutgoingTransactions.Count(i => i.Status == TransactionStatus.Confirmed)}"),
				];
				//f.Add(new ("Peers in/out/min/known",	$"{Connections.Count(i => i.InStatus == EstablishingStatus.Succeeded)}/{Connections.Count(i => i.OutStatus == EstablishingStatus.Succeeded)}/{Settings.PeersMin}/{Peers.Count}"));
				
				if(mcv != null)
				{
					f.Add(new ("Synchronization",		$"{mcv.Synchronization}"));
					f.Add(new ("Size",					$"{mcv.Size}"));
					f.Add(new ("Members",				$"{mcv.LastConfirmedRound?.Members.Count}"));
					f.Add(new ("Emission",				$"{mcv.LastConfirmedRound?.Emission.ToDecimalString()}"));
					f.Add(new ("ExeunitMinFee",			$"{mcv.LastConfirmedRound?.ConsensusExeunitFee.ToDecimalString()}"));
					f.Add(new ("SyncCache Blocks",		$"{mcv.SyncTail.Sum(i => i.Value.Count)}"));
					f.Add(new ("Loaded Rounds",			$"{mcv.LoadedRounds.Count}"));
					f.Add(new ("Last Non-Empty Round",	$"{(mcv.LastNonEmptyRound != null ? mcv.LastNonEmptyRound.Id : null)}"));
					f.Add(new ("Last Payload Round",	$"{(mcv.LastPayloadRound != null ? mcv.LastPayloadRound.Id : null)}"));
					f.Add(new ("Base Hash",				mcv.BaseHash.ToHex()));

					if(mcv.Synchronization == Synchronization.Synchronized)
					{
						string formatbalance(AccountAddress a)
						{
							return mcv.Accounts.Find(a, mcv.LastConfirmedRound.Id)?.Balance.ToDecimalString();
						}
	
						foreach(var i in mcv.Sun.Vault.Wallets)
						{
							var a = i.Key.ToString();
							f.Add(new ($"{a.Substring(0, 8)}...{a.Substring(a.Length-8, 8)} {(mcv.Sun.Vault.IsUnlocked(i.Key) ? "Unlocked" : "Locked")}", $"{formatbalance(i.Key),23}"));
						}
	
						if(SunGlobals.UI)
						{
						}
					}
				}
				else
				{
					//f.Add(new ("Members (retrieved)", $"{Members.Count}"));

					foreach(var i in mcv.Sun.Vault.Wallets)
					{
						f.Add(new ($"Account", $"{i}"));
					}
				}

				mcv.Sun.Statistics.Reset();
		
				return new SummaryApc.Return{Summary = f.Take(Limit).Select(i => new [] {i.Key, i.Value}).ToArray() }; 
			}
		}
	}

	public class ChainReportApc : McvApc
	{
		public int		Limit  { get; set; }

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Sun.Lock)
				return new Return {Rounds = mcv.Tail.Take(Limit)
													.Reverse()
													.Select(i => new Return.Round
																{
																	Id = i.Id, 
																	Members = i.Members == null ? 0 : i.Members.Count,
																	Confirmed = i.Confirmed,
																	Time = i.ConsensusTime,
																	Hash = i.Hash,
																	Votes = i.Votes.Select(b => new Return.Vote {	Generator = b.Generator, 
																																IsPayload = b.Transactions.Any(), 
																																/*Confirmed = i.Confirmed && i.Transactions.Any() && i.ConfirmedPayloads.Contains(b)*/ }),
																	JoinRequests = i.Transactions.SelectMany(i => i.Operations).OfType<CandidacyDeclaration>().Select(i => i.Transaction.Signer),
																})
													.ToArray()}; 
		}

		public class Return
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
	}

	public class VotesReportApc : McvApc
	{
		public int		RoundId  { get; set; }
		public int		Limit  { get; set; }

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Sun.Lock)
				return new VotesReportResponse{Votes = mcv	.FindRound(RoundId)?.Votes
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

	public class TransactApc : McvApc
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }
		public TransactionStatus		Await  { get; set; } = TransactionStatus.Confirmed;

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			return mcv.Transact(Operations, By, Await, workflow).Select(i => i.Flow.Log.Messages.Select(i => i.ToString()));
		}
	}

	public class EstimateOperationApc : McvApc
	{
		public IEnumerable<Operation>	Operations { get; set; }
		public AccountAddress			By  { get; set; }

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			var t = new Transaction {Zone = mcv.Zone, Operations = Operations.ToArray()};
			t.Sign(mcv.Sun.Vault.GetKey(By), []);

			return mcv.Call(() => new AllocateTransactionRequest {Transaction = t}, workflow);
		}
	}

	public class ApcTransaction
	{
		public int						Nid { get; set; }
		public TransactionId			Id { get; set; }
		public bool						Successful { get; set; }
			
		public EntityId					Member { get; set; }
		public int						Expiration { get; set; }
		public byte[]					PoW { get; set; }
		public byte[]					Tag { get; set; }
		public Money					Fee { get; set; }
		public byte[]					Signature { get; set; }
			 
		public AccountAddress			Signer { get; set; }
		public TransactionStatus		Status { get; set; }
		public IPAddress				MemberNexus { get; set; }
		public TransactionStatus		__ExpectedStatus { get; set; }

		public IEnumerable<Operation>	Operations  { get; set; }

		public ApcTransaction()
		{
		}

		public ApcTransaction(Transaction transaction)
		{
			Nid					= transaction.Nid;
			Id					= transaction.Id;
			Operations			= transaction.Operations.ToArray();
			Successful			= transaction.Successful;
			   
			Member				= transaction.Generator;
			Expiration			= transaction.Expiration;
			PoW					= transaction.PoW;
			Tag					= transaction.Tag;
			Fee					= transaction.Fee;
			Signature			= transaction.Signature;
			   
			MemberNexus			= (transaction.Rdi as Peer)?.IP ?? (transaction.Rdi as Sun)?.IP;
			Signer				= transaction.Signer;
			Status				= transaction.Status;
			__ExpectedStatus	= transaction.__ExpectedStatus;
		}
	}

	public class IncomingTransactionsApc : McvApc
	{
		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Sun.Lock)
				return mcv.IncomingTransactions.Select(i => new ApcTransaction(i)).ToArray();
		}
	}

	public class OutgoingTransactionsApc : McvApc
	{
		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Lock)
				return mcv.OutgoingTransactions.Select(i => new ApcTransaction(i)).ToArray();
		}
	}

	public class RdcApc : McvApc
	{
		public RdcRequest Request { get; set; }

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			try
			{
				return mcv.Call(() => Request, workflow);
			}
			catch(SunException ex)
			{
				var rp = RdcResponse.FromType(Request.Class);
				rp.Error = ex;
				
				return rp;
			}
		}
	}

	public class SetGeneratorApc : McvApc
	{
		public IEnumerable<AccountAddress>	 Generators {get; set;}

		public override object Execute(Mcv mcv, HttpListenerRequest request, HttpListenerResponse response, Flow workflow)
		{
			lock(mcv.Sun.Lock)
				mcv.Settings.Generators = Generators.ToList();

			return null;
		}
	}
}
