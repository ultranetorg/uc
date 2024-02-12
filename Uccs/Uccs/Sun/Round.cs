using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class Round : IBinarySerializable
	{
		public int											Id;
		public int											ParentId => Id - Mcv.P;
		public int											VotersRound => Id - Mcv.DeclareToGenerateDelay;
		public Round										Previous =>	Mcv.FindRound(Id - 1);
		public Round										Next =>	Mcv.FindRound(Id + 1);
		public Round										Parent => Mcv.FindRound(ParentId);
		public Round										Child => Mcv.FindRound(Id + Mcv.P);
		public int											TransactionsPerVoteExecutionLimit		=> Mcv.Zone.TransactionsPerRoundLimit / Members.Count;
		public int											TransactionsPerVoteAllowableOverflow	=> TransactionsPerVoteExecutionLimit * Mcv.Zone.TransactionsPerVoteAllowableOverflowMuliplier;
		public int											OperationsPerVoteLimit					=> Mcv.Zone.OperationsPerRoundLimit / Members.Count;

		public int											Try = 0;
		public DateTime										FirstArrivalTime = DateTime.MaxValue;

		public List<Vote>									Votes = new();
		public IEnumerable<Vote>							VotesOfTry => Votes.Where(i => i.Try == Try);
		public IEnumerable<Vote>							Payloads => VotesOfTry.Where(i => i.Transactions.Any());
		public IEnumerable<Vote>							Eligible 
															{ 
																get 
																{ 
																	var v = Mcv.VotersOf(this);
																	return VotesOfTry.Where(i => v.Any(j => j.Account == i.Generator)).GroupBy(i => i.Generator).Where(i => i.Count() == 1).Select(i => i.First());
																} 
															}
		public IGrouping<byte[], Vote>						Majority => Eligible.GroupBy(i => i.ParentHash, Bytes.EqualityComparer).MaxBy(i => i.Count());

		public IEnumerable<Transaction>						OrderedTransactions => Payloads.OrderBy(i => i.Generator).SelectMany(i => i.Transactions);
		public IEnumerable<Transaction>						Transactions => Confirmed ? ConfirmedTransactions : OrderedTransactions;

		public Time											ConfirmedTime;
		public Transaction[]								ConfirmedTransactions = {};
		public AccountAddress[]								ConfirmedMemberLeavers = {};
		public AccountAddress[]								ConfirmedAnalyzerJoiners = {};
		public AccountAddress[]								ConfirmedAnalyzerLeavers = {};
		public AccountAddress[]								ConfirmedFundJoiners = {};
		public AccountAddress[]								ConfirmedFundLeavers = {};
		public AccountAddress[]								ConfirmedViolators = {};
		public OperationId[]								ConfirmedEmissions = {};
		public OperationId[]								ConfirmedDomainBids = {};
		public Money										ConfirmedExeunitMinFee;
		public int											ConfirmedOverflowRound;

		public bool											Confirmed = false;
		public byte[]										Hash;

		public Money										Fees;
		public Money										Emission;
		public List<Member>									Members = new();
		public List<Analyzer>								Analyzers;
		public List<AccountAddress>							Funds;
		public List<Emission>								Emissions;
		public List<AuthorBid>								DomainBids;
		public int											AnalyzersIdCounter;
		public Dictionary<byte[], int>						NextAccountIds;
		public Dictionary<byte[], int>						NextAuthorIds;
		public Dictionary<byte[], int>						NextAnalysisIds;

		public Dictionary<AccountAddress, AccountEntry>		AffectedAccounts = new();
		public Dictionary<string, AuthorEntry>				AffectedAuthors = new();
		public Dictionary<ReleaseAddress, ReleaseEntry>		AffectedReleases = new();

		public Mcv											Mcv;

		public int RequiredVotes
		{
			get
			{ 
				var m = Mcv.VotersOf(this);

				int q;

				if(m.Count() == 1)		q = 1;
				else if(m.Count() == 2)	q = 2;
				else if(m.Count() == 4)	q = 3;
				else
					q = m.Count() * 2 / 3;

				return q;
			}
		}

		public bool ConsensusReached
		{
			get
			{ 
				int q = RequiredVotes;

				if(Eligible.Count() < q)
					return false;

				return Majority.Count() >= q;
			}
		}


		public Round(Mcv c)
		{
			Mcv = c;
		}
		public override string ToString()
		{
			return $"Id={Id}, VoT/P={Votes.Count}({VotesOfTry.Count()}/{Payloads.Count()}), Members={Members?.Count}, ConfirmedTime={ConfirmedTime}, {(Confirmed ? "Confirmed " : "")}, Hash={Hash?.ToHex()}";
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

		public AccountEntry AffectAccount(AccountAddress account/*, Operation operation*/)
		{
			if(AffectedAccounts.TryGetValue(account, out AccountEntry a))
				return a;
			
			var e = Mcv.Accounts.Find(account, Id - 1);	

			if(e != null)
				return AffectedAccounts[account] = e.Clone();
			else
			{
				var ci = Mcv.Accounts.KeyToCluster(account).ToArray();
				var c = Mcv.Accounts.Clusters.Find(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextAccountIds[ci] = 0;
				else
					NextAccountIds[ci] = c.NextEntityId;
				
				ai = NextAccountIds[ci]++;

				return AffectedAccounts[account] = new AccountEntry(Mcv) {Id = new EntityId(ci, ai), Address = account};
			}
		}

		public AuthorEntry AffectAuthor(string author)
		{
			if(AffectedAuthors.TryGetValue(author, out AuthorEntry a))
				return a;
			
			var e = Mcv.Authors.Find(author, Id - 1);

			if(e != null)
				return AffectedAuthors[author] = e.Clone();
			else
			{
				var ci = Mcv.Authors.KeyToCluster(author).ToArray();
				var c = Mcv.Authors.Clusters.Find(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextAccountIds[ci] = 0;
				else
					NextAccountIds[ci] = c.NextEntityId;
				
				ai = NextAccountIds[ci]++;

				return AffectedAuthors[author] = new AuthorEntry(Mcv){Id = new EntityId(ci, ai), Name = author};
			}
		}

		public ReleaseEntry AffectRelease(ReleaseAddress release)
		{
			if(AffectedReleases.TryGetValue(release, out ReleaseEntry a))
				return a;
			
			var e = Mcv.Releases.Find(release, Id - 1);

			if(e != null)
				return AffectedReleases[release] = e.Clone();
			else
			{
				var ci = Mcv.Releases.KeyToCluster(release).ToArray();
				var c = Mcv.Releases.Clusters.Find(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextAccountIds[ci] = 0;
				else
					NextAccountIds[ci] = c.NextEntityId;
				
				ai = NextAccountIds[ci]++;

				return AffectedReleases[release] = new ReleaseEntry(Mcv) {	Id = new EntityId(ci, ai), 
																			Address = release, 
																			Results = new AnalyzerResult[0]};
			}
		}

		public void Hashify()
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			w.Write(Mcv.BaseHash);
			w.Write(Id > 0 ? Previous.Hash : Mcv.Zone.Cryptography.ZeroHash);
			WriteConfirmed(w);

			Hash = Mcv.Zone.Cryptography.Hash(s.ToArray());
		}

		public void WriteBaseState(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(Id);
			writer.Write(Hash);
			writer.Write(ConfirmedTime);
			writer.Write(ConfirmedExeunitMinFee);
			writer.Write7BitEncodedInt(ConfirmedOverflowRound);
			writer.Write(Emission);
			writer.Write(Members, i => i.WriteBaseState(writer));
			writer.Write(Analyzers, i => i.WriteBaseState(writer));
			writer.Write7BitEncodedInt(AnalyzersIdCounter);
			writer.Write(Funds);
			writer.Write(Emissions, i => i.WriteBaseState(writer));
			writer.Write(DomainBids, i => i.WriteBaseState(writer));
		}

		public void ReadBaseState(BinaryReader reader)
		{
			Id						= reader.Read7BitEncodedInt();
			Hash					= reader.ReadHash();
			ConfirmedTime			= reader.Read<Time>();
			ConfirmedExeunitMinFee	= reader.ReadMoney();
			ConfirmedOverflowRound	= reader.Read7BitEncodedInt();
			Emission				= reader.ReadMoney();
			Members					= reader.Read<Member>(m => m.ReadBaseState(reader)).ToList();
			Analyzers				= reader.Read<Analyzer>(m => m.ReadBaseState(reader)).ToList();
			AnalyzersIdCounter		= reader.Read7BitEncodedInt();
			Funds					= reader.ReadList<AccountAddress>();
			Emissions				= reader.Read<Emission>(m => m.ReadBaseState(reader)).ToList();
			DomainBids				= reader.Read<AuthorBid>(m => m.ReadBaseState(reader)).ToList();
		}

		public void WriteConfirmed(BinaryWriter writer)
		{
			writer.Write(ConfirmedTime);
			writer.Write(ConfirmedExeunitMinFee);
			writer.Write7BitEncodedInt(ConfirmedOverflowRound);
			writer.Write(ConfirmedMemberLeavers);
			writer.Write(ConfirmedAnalyzerJoiners);
			writer.Write(ConfirmedAnalyzerLeavers);
			writer.Write(ConfirmedFundJoiners);
			writer.Write(ConfirmedFundLeavers);
			writer.Write(ConfirmedViolators);
			writer.Write(ConfirmedEmissions);
			writer.Write(ConfirmedDomainBids);
			writer.Write(ConfirmedTransactions, i => i.WriteConfirmed(writer));
		}

		public void ReadConfirmed(BinaryReader reader)
		{
			ConfirmedTime				= reader.Read<Time>();
			ConfirmedExeunitMinFee		= reader.ReadMoney();
			ConfirmedOverflowRound		= reader.Read7BitEncodedInt();
			ConfirmedMemberLeavers		= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerJoiners	= reader.ReadArray<AccountAddress>();
			ConfirmedAnalyzerLeavers	= reader.ReadArray<AccountAddress>();
			ConfirmedFundJoiners		= reader.ReadArray<AccountAddress>();
			ConfirmedFundLeavers		= reader.ReadArray<AccountAddress>();
			ConfirmedViolators			= reader.ReadArray<AccountAddress>();
			ConfirmedEmissions			= reader.ReadArray<OperationId>();
			ConfirmedDomainBids			= reader.ReadArray<OperationId>();
			ConfirmedTransactions		= reader.Read(() =>	new Transaction {Round = this}, t => t.ReadConfirmed(reader)).ToArray();
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
				w.Write(Hash);
				//w.Write(JoinRequests, i => i.Write(w)); /// for [LastConfimed-Pitch..LastConfimed]
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
				Hash = r.ReadHash();
				//JoinRequests.AddRange(r.ReadArray(() =>	{
				//											var b = new MemberJoinOperation();
				//											b.RoundId = Id;
				//											b.Read(r, Mcv.Zone);
				//											return b;
				//										}));
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
												i.Zone = Mcv.Zone;
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

			Hash = r.ReadHash();
		}
	}
}
