﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Net;
using Nethereum.Signer;

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

		public List<BlockPiece>									BlockPieces = new();

		public List<Block>										Blocks = new();
		public IEnumerable<JoinMembersRequest>					JoinRequests	=> Blocks.OfType<JoinMembersRequest>().GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
		public IEnumerable<Vote>								Votes			=> Blocks.OfType<Vote>().Where(i => i.Try == Try);
		public IEnumerable<Payload>								Payloads		=> Votes.OfType<Payload>();
		public IEnumerable<AccountAddress>						Forkers			=> Votes.GroupBy(i => i.Generator).Where(i => i.Count() > 1).Select(i => i.Key);
		public IEnumerable<Vote>								Unique			=> Votes.GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
		public IEnumerable<Vote>								Majority		=> Unique.Any() ? Unique.GroupBy(i => i.Consensus).Aggregate((i, j) => i.Count() > j.Count() ? i : j) : new Vote[0];

		public IEnumerable<AccountAddress>						ElectedJoiners		=> Majority.SelectMany(i => i.Joiners).Distinct().Where(j => Majority.Count(b => b.Joiners.Contains(j)) >= Database.VoterOf(Id).Count * 2 / 3);
		public IEnumerable<AccountAddress>						ElectedLeavers		=> Majority.SelectMany(i => i.Leavers).Distinct().Where(l => Majority.Count(b => b.Leavers.Contains(l)) >= Database.VoterOf(Id).Count * 2 / 3);
		public IEnumerable<AccountAddress>						ElectedViolators	=> Majority.SelectMany(i => i.Violators).Distinct().Where(v => Majority.Count(b => b.Violators.Contains(v)) >= Database.VoterOf(Id).Count * 2 / 3);
		public IEnumerable<AccountAddress>						ElectedFundJoiners	=> Majority.SelectMany(i => i.FundJoiners).Distinct().Where(j => Majority.Count(b => b.FundJoiners.Contains(j)) >= Database.MembersMax * 2 / 3);
		public IEnumerable<AccountAddress>						ElectedFundLeavers	=> Majority.SelectMany(i => i.FundLeavers).Distinct().Where(l => Majority.Count(b => b.FundLeavers.Contains(l)) >= Database.MembersMax * 2 / 3);

		public List<Member>										Members = new();
		public List<AccountAddress>								Funds = new();
		public List<Payload>									ConfirmedPayloads;
		public List<AccountAddress>								ConfirmedViolators = new();
		public List<Member>										ConfirmedJoiners = new();
		public List<AccountAddress>								ConfirmedLeavers = new();
		public List<AccountAddress>								ConfirmedFundJoiners = new();
		public List<AccountAddress>								ConfirmedFundLeavers = new();

		public bool												Voted = false;
		public bool												Confirmed = false;
		public byte[]											Hash;

		public ChainTime										Time;
		public BigInteger										WeiSpent;
		public Coin												Factor;
		public Coin												Emission;

		public Dictionary<AccountAddress, AccountEntry>			AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>					AffectedAuthors = new();
		public Dictionary<ProductAddress, ProductEntry>			AffectedProducts = new();
		public Dictionary<PlatformAddress, PlatformEntry>		AffectedPlatforms = new();
		public Dictionary<ReleaseAddress, ReleaseEntry>			AffectedReleases = new();
		
		public Database											Database;
		
		public Round(Database c)
		{
			Database = c;
		}

		public override string ToString()
		{
			return $"Id={Id}, Blocks(V/P/J)={Blocks.Count}({Votes.Count()}/{Payloads.Count()}/{JoinRequests.Count()}), Pieces={BlockPieces.Count}, Members={Members?.Count}, Time={Time}, {(Voted ? "Voted " : "")}{(Confirmed ? "Confirmed " : "")}";
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
				AffectedAccounts[account] = e.Clone();
			else
				AffectedAccounts[account] = new AccountEntry(Database){Address = account};

			return AffectedAccounts[account];
		}

		public AuthorEntry AffectAuthor(string name)
		{
			var e = Database.Authors.Find(name, Id);

			if(e != null)
				AffectedAuthors[name] = e.Clone();
			else
				AffectedAuthors[name] = new AuthorEntry(Database){Name = name};

			return AffectedAuthors[name];
		}

		//public AuthorEntry FindAuthor(string name)
		//{
		//	if(AffectedAuthors.ContainsKey(name))
		//		return AffectedAuthors[name];
		//
		//	return Chain.Authors.Find(name, Id - 1);
		//}
		//

		public ProductEntry FindProduct(ProductAddress address)
		{
			if(AffectedProducts.ContainsKey(address))
				return AffectedProducts[address];

			return Database.Products.Find(address, Id - 1);
		}

		public PlatformEntry FindPlatform(PlatformAddress name)
		{
			if(AffectedPlatforms.ContainsKey(name))
				return AffectedPlatforms[name];
		
			return Database.Platforms.Find(name, Id - 1);
		}
		
		public ReleaseEntry FindRelease(ReleaseAddress name)
		{
			if(AffectedReleases.ContainsKey(name))
				return AffectedReleases[name];
		
			return Database.Releases.Find(name, Id - 1);
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

		public PlatformEntry AffectPlatform(PlatformAddress address)
		{
			var e = FindPlatform(address);

			if(e != null)
				AffectedPlatforms[address] = e.Clone();
			else
				AffectedPlatforms[address] = new PlatformEntry(){Address = address};

			return AffectedPlatforms[address];
		}

		public ReleaseEntry AffectRelease(ReleaseAddress address)
		{
			var e = FindRelease(address);

			if(e != null)
				AffectedReleases[address] = e.Clone();
			else
				AffectedReleases[address] = new ReleaseEntry(){Address = address};

			return AffectedReleases[address];
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

		void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(Time);
			writer.Write(ConfirmedPayloads, i => i.WriteConfirmed(writer));
			writer.Write(ConfirmedViolators.OrderBy(i => i));
			writer.Write(ConfirmedJoiners.OrderBy(i => i.Generator), i => { writer.Write(i.Generator); writer.Write(i.IPs, i => writer.Write(i)); });
			writer.Write(ConfirmedLeavers.OrderBy(i => i));
			writer.Write(ConfirmedFundJoiners.OrderBy(i => i));
			writer.Write(ConfirmedFundLeavers.OrderBy(i => i));
		}

		void ReadConfirmed(BinaryReader reader)
		{
			Time		= reader.ReadTime();
			Blocks		= reader.ReadList(() =>	{	
													var b = new Payload(Database);											
													b.RoundId = Id;
													b.Round = this;
													b.Confirmed = true;

													b.ReadConfirmed(reader);

													return b as Block;
												});
	
			ConfirmedPayloads		= Blocks.Cast<Payload>().ToList();
			ConfirmedViolators		= reader.ReadList<AccountAddress>();
			ConfirmedJoiners		= reader.ReadList<Member>(() => new Member {Generator = reader.ReadAccount(), IPs = reader.ReadArray<IPAddress>(() => reader.ReadIPAddress())});
			ConfirmedLeavers		= reader.ReadList<AccountAddress>();
			ConfirmedFundJoiners	= reader.ReadList<AccountAddress>();
			ConfirmedFundLeavers	= reader.ReadList<AccountAddress>();
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
				w.Write(Blocks, i => {
										w.Write(i.TypeCode); 
										i.Write(w); 
									 });
				w.Write(BlockPieces);
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
				Blocks.AddRange(r.ReadArray(() =>	{
														var b = new JoinMembersRequest(Database);
														b.RoundId = Id;
														b.Round = this;
														b.Read(r);
														return b;
													}));
			} 
			else
			{
				Blocks	= r.ReadList(() =>	{
												var b = Block.FromType(Database, (BlockType)r.ReadByte());
												b.RoundId = Id;
												b.Round = this;
												b.Read(r);
												return b;
											});
				BlockPieces = r.ReadList(() =>	{
													var p = new BlockPiece(Database.Zone);
													p.Read(r);
													return p;
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