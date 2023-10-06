using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class Round : IBinarySerializable
	{
		public int											Id;
		public int											ParentId => Id - Mcv.Pitch;
		public Round										Previous =>	Mcv.FindRound(Id - 1);
		public Round										Next =>	Mcv.FindRound(Id + 1);
		public Round										Parent => Mcv.FindRound(ParentId);
		public int											TransactionsPerVoteGuaranteedLimit	=> Mcv.Zone.TransactionsPerRoundLimit / Members.Count;
		public int											TransactionsPerVoteAbsoluteLimit	=> Mcv.Zone.TransactionsPerRoundLimit / Members.Count * 10;
		public int											OperationsCountPerVoteLimit			=> Mcv.Zone.OperationsPerRoundLimit / Members.Count;

		public int											Try = 0;
		public DateTime										FirstArrivalTime = DateTime.MaxValue;

		public List<Vote>									Votes = new();
		public List<AnalyzerVoxRequest>						AnalyzerVoxes = new();
		public List<MemberJoinRequest>						JoinRequests = new();
		public IEnumerable<Vote>							VotesOfTry => Votes.Where(i => i.Try == Try);
		public IEnumerable<Vote>							Payloads => VotesOfTry.Where(i => i.Transactions.Any());
		public IEnumerable<Vote>							Unique => VotesOfTry.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());

		public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
		public IEnumerable<Transaction>						Transactions => Confirmed ? ConfirmedTransactions : OrderedTransactions;


		public ChainTime									ConfirmedTime;
		public Transaction[]								ConfirmedTransactions = {};
		public AccountAddress[]								ConfirmedMemberJoiners = {};
		public AccountAddress[]								ConfirmedMemberLeavers = {};
		public AccountAddress[]								ConfirmedAnalyzerJoiners = {};
		public AccountAddress[]								ConfirmedAnalyzerLeavers = {};
		public AccountAddress[]								ConfirmedFundJoiners = {};
		public AccountAddress[]								ConfirmedFundLeavers = {};
		public AccountAddress[]								ConfirmedViolators = {};
		public OperationId[]								ConfirmedEmissions = {};
		public OperationId[]								ConfirmedDomainBids = {};
		public AnalysisConclusion[]							ConfirmedAnalyses = {};

		public bool											Voted = false;
		public bool											Confirmed = false;
		public byte[]										Hash;
		public byte[]										Summary;

		public Money										Fees;
		//public Coin										TransactionPerByteFee;
		//public int										TransactionThresholdExcessRound;
		public Money										Emission;
		//public BigInteger									WeiSpent;
		//public Coin										Factor;
		public List<Member>									Members = new();
		public List<Emission>								Emissions = new ();
		public List<AuthorBid>								DomainBids = new ();
		public List<Analyzer>								Analyzers = new();
		public List<AccountAddress>							Funds = new();

		public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>				AffectedAuthors = new();
		
		public Mcv											Mcv;

		public Round(Mcv c)
		{
			Mcv = c;
		}

		public override string ToString()
		{
			return $"Id={Id}, Votes(VoT/P)={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), JR={JoinRequests.Count}, Members={Members?.Count}, ConfirmedTime={ConfirmedTime}, {(Voted ? "Voted " : "")}{(Confirmed ? "Confirmed " : "")}";
		}

		public void Distribute(Money amount, IEnumerable<AccountAddress> a)
		{
			if(a.Any())
			{
				var x = amount/a.Count();
	
				foreach(var i in a.Skip(1))
					AffectAccount(i).Balance += x;
	
				AffectAccount(a.First()).Balance += amount - (x * (a.Count() - 1));
			}
		}

		public void Distribute(Money amount, IEnumerable<AccountAddress> a, int ashare, IEnumerable<AccountAddress> b, int bshare)
		{
			var s = amount * new Money(ashare)/new Money(ashare + bshare);

			if(a.Any())
			{
				var x = s/a.Count();
		
				foreach(var i in a.Skip(1))
				{
					//RewardOrPay(i, x);
					AffectAccount(i).Balance += x;
				}

				var v = s - (x * (a.Count() - 1));
				
				//RewardOrPay(a.First(), v);
				AffectAccount(a.First()).Balance += v;
			}

			if(b.Any())
			{
				s = amount - s;
				var x = s/b.Count();
		
				foreach(var i in b.Skip(1))
				{
					//RewardOrPay(i, x);
					AffectAccount(i).Balance += x;
				}

				var v = s - (x * (b.Count() - 1));
				//RewardOrPay(b.First(), v);
				AffectAccount(b.First()).Balance += v;
			}
		}
		
		//public Operation GetMutable(int rid, Func<Operation, bool> op)
		//{
		//	var r = Chain.FindRound(rid);
		//
 		//	foreach(var b in r.Payloads)
 		//		foreach(var t in b.Transactions)
 		//			foreach(var o in t.Operations.Where(i => i.Mutable))
 		//				if(op(o))
		//				{	
		//					if(!AffectedMutables.Contains(o))
		//						AffectedMutables.Add(o);
		//					
		//					return o; 
		//				}
		//
		//	return null;
		//}
// 
// 		public void RewardOrPay(Account account, Coin fee)
// 		{
// 			if(Fees.ContainsKey(account))
// 				Fees[account] += fee; 
// 			else
// 				Fees[account] = fee;
// 
// 		}

		public AccountEntry AffectAccount(AccountAddress account/*, Operation operation*/)
		{
			if(AffectedAccounts.TryGetValue(account, out AccountEntry a))
				return a;
			
			var e = Mcv.Accounts.Find(account, Id - 1);	

			if(e != null)
				return AffectedAccounts[account] = e.Clone();
			else
				return AffectedAccounts[account] = new AccountEntry(Mcv) {Address = account};
		}

		public AuthorEntry AffectAuthor(string author)
		{
			if(AffectedAuthors.TryGetValue(author, out AuthorEntry a))
				return a;
			
			var e = Mcv.Authors.Find(author, Id - 1);

			if(e != null)
				return AffectedAuthors[author] = e.Clone();
			else
				return AffectedAuthors[author] = new AuthorEntry(Mcv){Name = author};
		}

 		public O FindOperation<O>(Func<O, bool> f) where O : Operation
 		{
 			foreach(var b in Payloads)
 				foreach(var t in b.Transactions)
 					foreach(var o in t.Operations.OfType<O>())
 						if(f(o))
 							return o;
 
 			return null;
 		}

 		public bool AnyOperation(Func<Operation, bool> f)
 		{
 			foreach(var b in Payloads)
 				foreach(var t in b.Transactions)
 					foreach(var o in t.Operations)
 						if(f(o))
 							return true;
 
 			return false;
 		}

 		public List<Transaction> FindTransactions(Func<Transaction, bool> f)
 		{
 			var o = new List<Transaction>();
 
 			foreach(var b in Payloads)
 				foreach(var t in b.Transactions)
 					if(f(t))
 						o.Add(t);
 
 			return o;
 		}

		public void Hashify(byte[] previous)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Mcv.BaseHash);
			w.Write(previous);

			WriteConfirmed(w);

			Hash = Mcv.Zone.Cryptography.Hash(s.ToArray());
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Id);
			writer.Write(Hash);
			writer.Write(ConfirmedTime);
			writer.Write(Emission);
			//writer.Write(TransactionPerByteFee);
			//writer.Write7BitEncodedInt(TransactionThresholdExcessRound);
			writer.Write(Members, i => i.WriteBaseState(writer));
			writer.Write(Analyzers, i => i.WriteBaseState(writer));
			writer.Write(Funds);
			writer.Write(Emissions, i => i.WriteBaseState(writer));
			writer.Write(DomainBids, i => i.WriteBaseState(writer));
		}

		public void ReadBaseState(BinaryReader reader)
		{
			Id									= reader.Read7BitEncodedInt();
			Hash								= reader.ReadSha3();
			ConfirmedTime						= reader.ReadTime();
			Emission							= reader.ReadCoin();
			//TransactionPerByteFee				= reader.ReadCoin();
			//TransactionThresholdExcessRound	= reader.Read7BitEncodedInt();
			Members								= reader.Read<Member>(m => m.ReadBaseState(reader)).ToList();
			Analyzers							= reader.Read<Analyzer>(m => m.ReadBaseState(reader)).ToList();
			Funds								= reader.ReadList<AccountAddress>();
			Emissions							= reader.Read<Emission>(m => m.ReadBaseState(reader)).ToList();
			DomainBids							= reader.Read<AuthorBid>(m => m.ReadBaseState(reader)).ToList();
		}

		public void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConfirmedTime);
			writer.Write(ConfirmedMemberJoiners);
			writer.Write(ConfirmedMemberLeavers);
			writer.Write(ConfirmedAnalyzerJoiners);
			writer.Write(ConfirmedAnalyzerLeavers);
			writer.Write(ConfirmedFundJoiners);
			writer.Write(ConfirmedFundLeavers);
			writer.Write(ConfirmedViolators);
			writer.Write(ConfirmedEmissions);
			writer.Write(ConfirmedDomainBids);
			writer.Write(ConfirmedAnalyses);
			writer.Write(ConfirmedTransactions, i => i.WriteConfirmed(writer));
		}

		void ReadConfirmed(BinaryReader reader)
		{
			ConfirmedTime				= reader.ReadTime();
			ConfirmedMemberJoiners		= reader.ReadArray<AccountAddress>();
			ConfirmedMemberLeavers		= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerJoiners	= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerLeavers	= reader.ReadArray<AccountAddress>();
			ConfirmedFundJoiners		= reader.ReadArray<AccountAddress>();
			ConfirmedFundLeavers		= reader.ReadArray<AccountAddress>();
			ConfirmedViolators			= reader.ReadArray<AccountAddress>();
			ConfirmedEmissions			= reader.ReadArray<OperationId>();
			ConfirmedDomainBids			= reader.ReadArray<OperationId>();
			ConfirmedAnalyses			= reader.ReadArray<AnalysisConclusion>();
			ConfirmedTransactions		= reader.Read(() =>	new Transaction(Mcv.Zone) {Round = this}, t => t.ReadConfirmed(reader)).ToArray();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Id);
			w.Write(Confirmed);
			
			if(Confirmed)
			{
// #if DEBUG
// 				w.Write(Hash);
// #endif
				WriteConfirmed(w);
				w.Write(JoinRequests, i => i.Write(w)); /// for [LastConfimed-Pitch..LastConfimed]
			} 
			else
			{
				w.Write(Votes, i => {
										i.WriteForRoundUnconfirmed(w); 
									 });
			}
		}

		public void Read(BinaryReader r)
		{
			Id			= r.Read7BitEncodedInt();
			Confirmed	= r.ReadBoolean();
			
			if(Confirmed)
			{
// #if DEBUG
// 				Hash = r.ReadSha3();
// #endif
				ReadConfirmed(r);
				JoinRequests.AddRange(r.ReadArray(() =>	{
															var b = new MemberJoinRequest();
															b.RoundId = Id;
															b.Read(r, Mcv.Zone);
															return b;
														}));
			} 
			else
			{
				Votes = r.ReadList(() => {
											var v = new Vote(Mcv);
											v.RoundId = Id;
											v.Round = this;
											v.ReadForRoundUnconfirmed(r);
												
											foreach(var i in v.Transactions)
											{
												i.Round = this;
											}

											return v;
										 });
			}
		}

		public void Save(BinaryWriter w)
		{
			WriteConfirmed(w);
			
			w.Write(Hash);
		}

		public void Load(BinaryReader r)
		{
			ReadConfirmed(r);

			Hash = r.ReadSha3();
		}
	}
}
