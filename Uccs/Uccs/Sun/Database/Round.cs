using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Net;
using Nethereum.Signer;
using static Uccs.Net.ChainReportResponse;
using System.Xml.Linq;
using NativeImport;

namespace Uccs.Net
{
	public class Round : IBinarySerializable
	{
		public int												Id;
		public int												ParentId => Id - Database.Pitch;
		public Round											Previous =>	Database.FindRound(Id - 1);
		public Round											Next =>	Database.FindRound(Id + 1);
		public Round											Parent => Database.FindRound(ParentId);

		public int												Try = 0;
		public DateTime											FirstArrivalTime = DateTime.MaxValue;

		public List<Vote>										Votes = new();
		public List<MemberJoinRequest>							JoinRequests = new();
		public IEnumerable<Vote>								VotesOfTry => Votes.Where(i => i.Try == Try);
		public IEnumerable<Vote>								Payloads => VotesOfTry.Where(i => i.Transactions.Any());
		public IEnumerable<Vote>								Unique => VotesOfTry.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
		public List<AnalyzerVoxRequest>							AnalyzerVoxes = new();

		public IEnumerable<Transaction>							OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);

		public List<Generator>									Members = new();
		public List<Analyzer>									Analyzers = new();
		public List<AccountAddress>								Funds = new();

		public ChainTime										ConfirmedTime;
		public Transaction[]									ConfirmedTransactions = {};
		public AccountAddress[]									ConfirmedGeneratorJoiners = {};
		public AccountAddress[]									ConfirmedGeneratorLeavers = {};
		public AccountAddress[]									ConfirmedAnalyzerJoiners = {};
		public AccountAddress[]									ConfirmedAnalyzerLeavers = {};
		public AccountAddress[]									ConfirmedFundJoiners = {};
		public AccountAddress[]									ConfirmedFundLeavers = {};
		public AccountAddress[]									ConfirmedViolators = {};
		public AnalysisConclusion[]								ConfirmedAnalyses = {};

		public bool												Voted = false;
		public bool												Confirmed = false;
		public byte[]											Hash;
		public byte[]											Summary;

		public BigInteger										WeiSpent;
		public Coin												Factor;
		public Coin												Emission;

		public Dictionary<AccountAddress, AccountEntry>			AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>					AffectedAuthors = new();
		public Dictionary<ResourceAddress, ResourceEntry>			AffectedReleases = new();
		
		public Database											Database;

		public Round(Database c)
		{
			Database = c;
		}

		public override string ToString()
		{
			return $"Id={Id}, Blocks(V/P/J)={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}/{JoinRequests.Count()}), Generators={Members?.Count}, ConfirmedTime={ConfirmedTime}, {(Voted ? "Voted " : "")}{(Confirmed ? "Confirmed " : "")}";
		}

		public void Distribute(Coin amount, IEnumerable<AccountAddress> a)
		{
			if(a.Any())
			{
				var x = amount/a.Count();
	
				foreach(var i in a.Skip(1))
					AffectAccount(i).Balance += x;
	
				AffectAccount(a.First()).Balance += amount - (x * (a.Count() - 1));
			}
		}

		public void Distribute(Coin amount, IEnumerable<AccountAddress> a, int ashare, IEnumerable<AccountAddress> b, int bshare)
		{
			var s = amount * new Coin(ashare)/new Coin(ashare + bshare);

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

		public AccountEntry AffectAccount(AccountAddress account)
		{
			if(AffectedAccounts.ContainsKey(account))
				return AffectedAccounts[account];
			
			var e = Database.Accounts.Find(account, Id - 1);

			if(e != null)
				return AffectedAccounts[account] = e.Clone();
			else
				return AffectedAccounts[account] = new AccountEntry(Database){Address = account};
		}

		public AuthorEntry AffectAuthor(string author)
		{
			if(AffectedAuthors.ContainsKey(author))
				return AffectedAuthors[author];
			
			var e = Database.Authors.Find(author, Id - 1);

			if(e != null)
				return AffectedAuthors[author] = e.Clone();
			else
				return AffectedAuthors[author] = new AuthorEntry(Database){Name = author};
		}

		//public AuthorEntry FindAuthor(string name)
		//{
		//	if(AffectedAuthors.ContainsKey(name))
		//		return AffectedAuthors[name];
		//
		//	return Chain.Authors.Find(name, Id - 1);
		//}
		//

		public AccountEntry FindAccount(AccountAddress account)
		{
			if(AffectedAccounts.ContainsKey(account))
				return AffectedAccounts[account];
		
			return Database.Accounts.Find(account, Id - 1);
		}

		public ResourceEntry FindRelease(ResourceAddress name)
		{
			if(AffectedReleases.ContainsKey(name))
				return AffectedReleases[name];
		
			return Database.Resources.Find(name, Id - 1);
		}

		public ResourceEntry AffectRelease(ResourceAddress release)
		{
			if(AffectedReleases.ContainsKey(release))
				return AffectedReleases[release];
			
			var e = Database.Resources.Find(release, Id - 1);
			
			if(e != null)
				return AffectedReleases[release] = e.Clone();
			else
				return AffectedReleases[release] = new ResourceEntry(){Address = release};
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

			w.Write(Database.BaseHash);
			w.Write(previous);

			WriteConfirmed(w);

			Hash = Database.Zone.Cryptography.Hash(s.ToArray());
		}

		public void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConfirmedTime);
			writer.Write(ConfirmedGeneratorJoiners);
			writer.Write(ConfirmedGeneratorLeavers);
			//writer.Write(ConfirmedHubJoiners);
			//writer.Write(ConfirmedHubLeavers);
			writer.Write(ConfirmedAnalyzerJoiners);
			writer.Write(ConfirmedAnalyzerLeavers);
			writer.Write(ConfirmedFundJoiners);
			writer.Write(ConfirmedFundLeavers);
			writer.Write(ConfirmedViolators);
			writer.Write(ConfirmedAnalyses);
			writer.Write(ConfirmedTransactions, i => i.WriteConfirmed(writer));
		}

		void ReadConfirmed(BinaryReader reader)
		{
			ConfirmedTime				= reader.ReadTime();
			ConfirmedGeneratorJoiners	= reader.ReadArray<AccountAddress>();
			ConfirmedGeneratorLeavers	= reader.ReadArray<AccountAddress>();
			//ConfirmedHubJoiners			= reader.ReadArray<AccountAddress>();
			//ConfirmedHubLeavers			= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerJoiners	= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerLeavers	= reader.ReadArray<AccountAddress>();
			ConfirmedFundJoiners		= reader.ReadArray<AccountAddress>();
			ConfirmedFundLeavers		= reader.ReadArray<AccountAddress>();
			ConfirmedViolators			= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyses			= reader.ReadArray<AnalysisConclusion>();
			ConfirmedTransactions		= reader.Read(() =>	new Transaction(Database.Zone), t => t.ReadConfirmed(reader)).ToArray();
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
																		b.Read(r, Database.Zone);
																		return b;
																	}));
			} 
			else
			{
				Votes	= r.ReadList(() =>	{
												var b = new Vote(Database);
												b.RoundId = Id;
												b.Round = this;
												b.ReadForRoundUnconfirmed(r);
												return b;
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
