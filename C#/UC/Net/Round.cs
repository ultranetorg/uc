using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Net;

namespace UC.Net
{
	public class Round : IBinarySerializable
	{
		public int										Id;
		public int										ParentId => Id - Roundchain.Pitch;
		public Round									Previous =>	Chain.FindRound(Id - 1);
		public Round									Next =>	Chain.FindRound(Id + 1);
		public Round									Parent => Chain.FindRound(ParentId);

		public int										Try = 0;
		public DateTime									FirstArrivalTime = DateTime.MaxValue;
		public DateTime									LastAccessed = DateTime.UtcNow;

		public List<Block>								Blocks = new();
		public IEnumerable<GeneratorJoinRequest>		JoinRequests	=> Blocks.OfType<GeneratorJoinRequest>();
		public IEnumerable<Vote>						Votes			=> Blocks.OfType<Vote>().Where(i => i.Try == Try);
		public IEnumerable<Payload>						Payloads		=> Votes.OfType<Payload>().OrderBy(i => i.OrderingKey, new BytesComparer());
		public IEnumerable<Account>						Forkers			=> Votes.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key);
		public IEnumerable<Vote>						Unique			=> Votes.OfType<Vote>().GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
		public IEnumerable<Vote>						Majority		=> Unique.Any() ? Unique.GroupBy(i => i.Reference).Aggregate((i, j) => i.Count() > j.Count() ? i : j) : new Vote[0];

		public IEnumerable<Account>						ElectedViolators	=> Majority.SelectMany(i => i.Violators).Distinct().Where(v => Majority.Count(b => b.Violators.Contains(v)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>						ElectedJoiners		=> Majority.SelectMany(i => i.Joiners).Distinct().Where(j => Majority.Count(b => b.Joiners.Contains(j)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>						ElectedLeavers		=> Majority.SelectMany(i => i.Leavers).Distinct().Where(l => Majority.Count(b => b.Leavers.Contains(l)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>						ElectedFundJoiners	=> Majority.SelectMany(i => i.FundJoiners).Distinct().Where(j => Majority.Count(b => b.FundJoiners.Contains(j)) >= Roundchain.MembersMax * 2 / 3);
		public IEnumerable<Account>						ElectedFundLeavers	=> Majority.SelectMany(i => i.FundLeavers).Distinct().Where(l => Majority.Count(b => b.FundLeavers.Contains(l)) >= Roundchain.MembersMax * 2 / 3);

		public List<Peer>								Members;
		public List<Account>							Funds;
		//public List<Peer>								Hubs;
		public IEnumerable<Payload>						ConfirmedPayloads => Payloads.Where(i => i.Confirmed);
		public List<Account>							ConfirmedViolators;
		public List<Account>							ConfirmedJoiners;
		public List<Account>							ConfirmedLeavers;
		public List<Account>							ConfirmedFundJoiners;
		public List<Account>							ConfirmedFundLeavers;

		public bool										Voted = false;
		public bool										Confirmed = false;
		public byte[]									Hash;

		public ChainTime								Time;
		public BigInteger								WeiSpent;
		public Coin										Factor;
		public Coin										Emission;

		public Dictionary<Account, AccountEntry>		AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>			AffectedAuthors = new();
		public Dictionary<ProductAddress, ProductEntry>	AffectedProducts = new(); /// needed to not load all producta with all releases when fetching an author
		//public HashSet<Round>							AffectedRounds = new();
		public IEnumerable<Payload>						ExecutingPayloads;
		public IEnumerable<Operation>					ExecutedOperations => ExecutingPayloads	.SelectMany(i => i.Transactions)
																								.SelectMany(i => i.Operations)
																								.Where(i => i.Executed);
		public Roundchain								Chain;
		
		public Round(Roundchain c)
		{
			Chain = c;
		}

		public override string ToString()
		{
			return $"Id={Id}, Blocks={Blocks.Count}, Payloads={Payloads.Count()}, Members={Members?.Count}, Time={Time.ToString()}, {(Voted ? "Voted " : "")}{(Confirmed ? "Confirmed " : "")}";
		}

		public void Distribute(Coin amount, IEnumerable<Account> a)
		{
			if(a.Any())
			{
				var x = amount/a.Count();
	
				foreach(var i in a.Skip(1))
					AffectAccount(i).Balance += x;
	
				AffectAccount(a.First()).Balance += amount - (x * (a.Count() - 1));
			}
		}

		public void Distribute(Coin amount, IEnumerable<Account> a, int ashare, IEnumerable<Account> b, int bshare)
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

		public AccountEntry AffectAccount(Account account)
		{
			if(AffectedAccounts.ContainsKey(account))
				return AffectedAccounts[account];

			var e = Chain.Accounts.Find(account, Id - 1);

			if(e != null)
				AffectedAccounts[account] = e.Clone();
			else
				AffectedAccounts[account] = new AccountEntry(Chain){Account = account};

			return AffectedAccounts[account];
		}

		public AuthorEntry AffectAuthor(string name)
		{
			var e = FindAuthor(name);

			if(e != null)
				AffectedAuthors[name] = e.Clone();
			else
				AffectedAuthors[name] = new AuthorEntry(Chain){Name = name};

			return AffectedAuthors[name];
		}

		public AuthorEntry FindAuthor(string name)
		{
			if(AffectedAuthors.ContainsKey(name))
				return AffectedAuthors[name];

			return Chain.Authors.Find(name, Id - 1);
		}

		public ProductEntry AffectProduct(ProductAddress address)
		{
			var e = FindProduct(address);

			if(e != null)
				AffectedProducts[address] = e.Clone();
			else
				AffectedProducts[address] = new ProductEntry(){Address = address};

			return AffectedProducts[address];
		}

		public ProductEntry FindProduct(ProductAddress address)
		{
			if(AffectedProducts.ContainsKey(address))
				return AffectedProducts[address];

			return Chain.FindProduct(address, Id - 1);
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

		public void Seal()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Id > 0 ? Previous.Hash : Cryptography.ZeroHash);

			WriteConfirmed(w);

			Hash = Cryptography.Current.Hash(s.ToArray());
		}

		void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Time);
			writer.Write(ConfirmedPayloads, i => i.WriteConfirmed(writer));
			writer.Write(ConfirmedViolators);
			writer.Write(ConfirmedJoiners);
			writer.Write(ConfirmedLeavers);
			writer.Write(ConfirmedFundJoiners);
			writer.Write(ConfirmedFundLeavers);
		}

		void ReadConfirmed(BinaryReader reader)
		{
			Time		= reader.ReadTime();
			Blocks		= reader.ReadList(() =>	{	
													var b = new Payload(Chain);											
													b.RoundId = Id;
													b.Round = this;
													b.Confirmed = true;

													b.ReadConfirmed(reader);

													return b as Block;
												});
	
			ConfirmedViolators		= reader.ReadList<Account>();
			ConfirmedJoiners		= reader.ReadList<Account>();
			ConfirmedLeavers		= reader.ReadList<Account>();
			ConfirmedFundJoiners	= reader.ReadList<Account>();
			ConfirmedFundLeavers	= reader.ReadList<Account>();
		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Id);
			w.Write(Confirmed);
			
			if(Confirmed)
			{
				WriteConfirmed(w);
			} 
			else
			{
				w.Write(Voted);
				w.Write(Blocks, i => {
										w.Write(i.TypeCode); 
										i.Write(w); 
									 });
			}
		}

		public void Read(BinaryReader r)
		{
			Id			= r.Read7BitEncodedInt();
			Confirmed	= r.ReadBoolean();
			
			if(Confirmed)
			{
				ReadConfirmed(r);
			} 
			else
			{
				Voted	= r.ReadBoolean();
				Blocks	= r.ReadList(() =>	{
												var b = Block.FromType(Chain, (BlockType)r.ReadByte());
												b.RoundId = Id;
												b.Round = this;
												b.Read(r);
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
