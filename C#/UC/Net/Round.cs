using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
//using Nethereum.Util;

namespace UC.Net
{
	public class Round : IBinarySerializable
	{
		public int							Id;
		public int							ParentId => Id - Roundchain.Pitch;

		public int							Try = 0;
		public DateTime						FirstArrivalTime = DateTime.MaxValue;
		public DateTime						LastAccessed = DateTime.UtcNow;

		public List<Block>					Blocks = new();
		public IEnumerable<JoinRequest>		JoinRequests	=> Blocks.OfType<JoinRequest>();
		public IEnumerable<Vote>			Votes			=> Blocks.OfType<Vote>().Where(i => i.Try == Try);
		public IEnumerable<Payload>			Payloads		=> Votes.OfType<Payload>().OrderBy(i => i.Signature, new BytesComparer());
		public IEnumerable<Account>			Forkers			=> Votes.GroupBy(i => i.Member).Where(i => i.Count() > 1).Select(i => i.Key);
		public IEnumerable<Vote>			Unique			=> Votes.OfType<Vote>().GroupBy(i => i.Member).Where(i => i.Count() == 1).Select(i => i.First());
		public IEnumerable<Vote>			Majority		=> Unique.Any() ? Unique.GroupBy(i => i.Reference).Aggregate((i, j) => i.Count() > j.Count() ? i : j) : new Vote[0];

		public IEnumerable<Account>			ElectedViolators			=> Majority.SelectMany(i => i.Violators).Distinct().Where(v => Majority.Count(b => b.Violators.Contains(v)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>			ElectedJoiners				=> Majority.SelectMany(i => i.Joiners).Distinct().Where(j => Majority.Count(b => b.Joiners.Contains(j)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>			ElectedLeavers				=> Majority.SelectMany(i => i.Leavers).Distinct().Where(l => Majority.Count(b => b.Leavers.Contains(l)) >= Majority.Count() * 2 / 3);
		public IEnumerable<Account>			ElectedFundableAssignments	=> Majority.SelectMany(i => i.FundableAssignments).Distinct().Where(j => Majority.Count(b => b.FundableAssignments.Contains(j)) >= Roundchain.MembersMax * 2 / 3);
		public IEnumerable<Account>			ElectedFundableRevocations	=> Majority.SelectMany(i => i.FundableRevocations).Distinct().Where(l => Majority.Count(b => b.FundableRevocations.Contains(l)) >= Roundchain.MembersMax * 2 / 3);

		public Dictionary<Account, AccountEntry>		Accounts = new();
		public Dictionary<string, AuthorEntry>			Authors = new();
		public Dictionary<ProductAddress, ProductEntry>	Products = new();

		public IEnumerable<Payload>			ConfirmedPayloads => Payloads.Where(i => i.Confirmed);
		public List<Account>				ConfirmedViolators = new();
		public List<Account>				ConfirmedJoiners = new();
		public List<Account>				ConfirmedLeavers = new();
		public List<Account>				ConfirmedFundableAssignments = new();
		public List<Account>				ConfirmedFundableRevocations = new();

		public List<Peer>					Members;
		public List<Account>				Fundables;

		public bool							Voted = false;
		public bool							Confirmed = false;
		public byte[]						Hash;

		public ChainTime					Time;
		public BigInteger					WeiSpent;
		public Coin							Factor;
		public Coin							Emission;

		public Roundchain					Chain;
		public Round						Previous =>	Chain.FindRound(Id - 1);
		public Round						Next =>	Chain.FindRound(Id + 1);
		public Round						Parent => Chain.FindRound(ParentId);

		public Operation					CurrentOperation;
		public IEnumerable<Payload>			ExecutingPayloads;
		public IEnumerable<Operation>		EffectiveOperations => CurrentOperation != null ? ExecutingPayloads.SelectMany(i => i.SuccessfulTransactions).
																												SelectMany(i => i.SuccessfulOperations).
																												SkipWhile(i => i != CurrentOperation).
																												Skip(1) 
																							:
																							(Confirmed ? ConfirmedPayloads : Payloads). SelectMany(i => i.SuccessfulTransactions).
																																		SelectMany(i => i.SuccessfulOperations);
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
					GetAccount(i).Balance += x;
	
				GetAccount(a.First()).Balance += amount - (x * (a.Count() - 1));
			}
		}

		public void Distribute(Coin amount, IEnumerable<Account> a, int ashare, IEnumerable<Account> b, int bshare)
		{
			var s = amount * new Coin(ashare)/new Coin(ashare + bshare);

			if(a.Any())
			{
				var x = s/a.Count();
		
				foreach(var i in a.Skip(1))
					GetAccount(i).Balance += x;
		
				GetAccount(a.First()).Balance += s - (x * (a.Count() - 1));
			}

			if(b.Any())
			{
				s = amount - s;
				var x = s/b.Count();
		
				foreach(var i in b.Skip(1))
					GetAccount(i).Balance += x;
		
				GetAccount(b.First()).Balance += s - (x * (b.Count() - 1));
			}
		}

		public AccountEntry GetAccount(Account account)
		{
			if(Accounts.ContainsKey(account))
				return Accounts[account];

			var e = Chain.Accounts.Find(account, Id - 1);

			if(e != null)
				Accounts[account] = e.Clone();
			else
				Accounts[account] = new AccountEntry(Chain, account);

			return Accounts[account];
		}

		public AuthorEntry GetAuthor(string name)
		{
			var e = FindAuthor(name);

			if(e != null)
				Authors[name] = e.Clone();
			else
				Authors[name] = new AuthorEntry(Chain, name);

			return Authors[name];
		}

		public AuthorEntry FindAuthor(string name)
		{
			if(Authors.ContainsKey(name))
				return Authors[name];

			return Chain.Authors.Find(name, Id - 1);
		}

		public ProductEntry GetProduct(ProductAddress name)
		{
			var e = FindProduct(name);

			if(e != null)
				Products[name] = e.Clone();
			else
				Products[name] = new ProductEntry(name);

			return Products[name];
		}

		public ProductEntry FindProduct(ProductAddress name)
		{
			if(Products.ContainsKey(name))
				return Products[name];

			return Chain.FindProduct(name, Id - 1);
		}


// 		public List<Operation> FindOperations(Func<Operation, bool> f)
// 		{
// 			var list = new List<Operation>();
// 
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					foreach(var o in t.Operations)
// 						if(f(o))
// 							list.Add(o);
// 
// 			return list;
// 		}

// 		public List<O> FindOperations<O>(Func<O, bool> f = null)
// 		{
// 			var list = new List<O>();
// 
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					foreach(var o in t.Operations.OfType<O>())
// 						if(f == null || f(o))
// 							list.Add(o);
// 
// 			return list;
// 		}


// 		public List<O> FindOperations<O>(Func<O, List<O>, bool> f = null)
// 		{
// 			var list = new List<O>();
// 
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					foreach(var o in t.Operations.OfType<O>())
// 						if(f == null || f(o, list))
// 							list.Add(o);
// 
// 			return list;
// 		}
// 
// 		public Operation FindOperation(Func<Operation, bool> f)
// 		{
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					foreach(var o in t.Operations)
// 						if(f(o))
// 							return o;
// 
// 			return null;
// 		}
// 
 		public O FindOperation<O>(Func<O, bool> f) where O : Operation
 		{
 			foreach(var b in Payloads)
 				foreach(var t in b.Transactions)
 					foreach(var o in t.Operations.OfType<O>())
 						if(f(o))
 							return o;
 
 			return null;
 		}
// 
// 		public O FindOperation<O>() where O : Operation
// 		{
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					foreach(var o in t.Operations.OfType<O>())
// 						return o;
// 
// 			return null;
// 		}
// 
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
// 
// 		public Transaction FindTransaction(Func<Transaction, bool> f)
// 		{
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					if(f(t))
// 						return t;
// 
// 			return null;
// 		}
// 
// 		public bool ContainsTransaction(Transaction t)
// 		{
// 			foreach(var b in Payloads)
// 				if(b.Transactions.Contains(t))
// 					return true;
// 
// 			return false;
// 		}
// 
// 		public bool AnyTransaction(Func<Transaction, bool> f)
// 		{
// 			foreach(var b in Payloads)
// 				foreach(var t in b.Transactions)
// 					if(f(t))
// 						return true;
// 
// 			return false;
// 		}

// 		public static byte[] CalculateHash(IEnumerable<Payload> blocks)
// 		{
// 			var s = new MemoryStream();
// 			var w = new BinaryWriter(s);
// 
// 			foreach(var i in blocks)
// 			{
// 				w.Write(i.Hash);
// 			}
// 			
// 			return Cryptography.Current.Hash((w.BaseStream as MemoryStream).ToArray());
// 		}

		public void Write(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Id);
			w.Write(Voted);
			w.Write(Confirmed);
			w.Write(Time);
		
			w.Write(Blocks,	 i => {w.Write((byte)i.Type); i.Write(w);});
			w.Write(ConfirmedJoiners);
			w.Write(ConfirmedLeavers);
			w.Write(ConfirmedViolators);
			w.Write(ConfirmedFundableAssignments);
			w.Write(ConfirmedFundableRevocations);
		}

		public void Read(BinaryReader r)
		{
			Id			= r.Read7BitEncodedInt();
			Voted		= r.ReadBoolean();
			Confirmed	= r.ReadBoolean();
			Time		= r.ReadTime();

			Blocks = r.ReadList(() =>	{
											var b = Block.FromType(Chain, (BlockType)r.ReadByte());
											b.Round = this;
											b.Read(r);
											return b;
										});

			ConfirmedJoiners = r.ReadList<Account>();
			ConfirmedLeavers = r.ReadList<Account>();
			ConfirmedViolators = r.ReadList<Account>();
			ConfirmedFundableAssignments = r.ReadList<Account>();
			ConfirmedFundableRevocations = r.ReadList<Account>();
		}


		public void Seal()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write7BitEncodedInt(Try);
			w.Write(Time);

			foreach(var i in ConfirmedJoiners)				w.Write(i);
			foreach(var i in ConfirmedLeavers)				w.Write(i);
			foreach(var i in ConfirmedViolators)			w.Write(i);
			foreach(var i in ConfirmedFundableAssignments)	w.Write(i);
			foreach(var i in ConfirmedFundableRevocations)	w.Write(i);

			w.Write(ConfirmedPayloads, i => w.Write(i.Signature));

			Hash = Cryptography.Current.Hash(s.ToArray());
		}

		public void Save(BinaryWriter w)
		{
			w.Write7BitEncodedInt(Try);
			w.Write(Time);
			w.Write(ConfirmedJoiners);
			w.Write(ConfirmedLeavers);
			w.Write(ConfirmedViolators);
			w.Write(ConfirmedFundableAssignments);
			w.Write(ConfirmedFundableRevocations);
		
			//ConfirmedPayloads.First().Reference.Write(w);

			w.Write(ConfirmedPayloads, i => i.Save(w));
		}

		public void Load(BinaryReader r)
		{
			Try = r.Read7BitEncodedInt();
			Time = r.ReadTime();
			ConfirmedJoiners = r.ReadList(() => r.ReadAccount());
			ConfirmedLeavers = r.ReadList(() => r.ReadAccount());
			ConfirmedViolators = r.ReadList(() => r.ReadAccount());
			ConfirmedFundableAssignments = r.ReadList(() => r.ReadAccount());
			ConfirmedFundableRevocations = r.ReadList(() => r.ReadAccount());

			//var rr = new RoundReference();
			//rr.Read(r);

			Blocks = r.ReadList(() =>	{
											var b =	new Payload(Chain)
														{
															Round = this,
															RoundId = Id,
															//Reference = rr,
															Confirmed = true
														};
											b.Load(r);
											return b as Block;
										});

			Seal();
		}
	}
}
