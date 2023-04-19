using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nethereum.ABI.Util;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.RPC.Accounts;
using Nethereum.Signer;
using Nethereum.Util;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using RocksDbSharp;

namespace Uccs.Net
{
	public delegate void BlockDelegate(Block b);

	public class Database
	{
		//public ZoneGenesis					Genesis => Genesises.Find(i => i.Zone == Zone.Current && i.Crypto.GetType() == Cryptography.Current.GetType());

		public const int					Pitch = 8;
		public const int					LastGenesisRound = Pitch * 3;
		public const int					MembersMin = 7;
		public const int					MembersMax = 1024;
		public const int					NewMembersPerRoundMax = 1;
		public const int					MembersRotation = 32;
		public  int							TailLength => Dev != null && Dev.TailLength100 ? 100 : 1000;
		const int							LoadedRoundsMax = 1000;
		public static readonly Coin			BailMin = 1000;
		public static readonly Coin			FeePerByte = new Coin(0.000001);

		public Zone							Zone;
		public DatabaseSettings				Settings;
		DevSettings							Dev;
		public Role							Roles => (Settings.Base ? Role.Base : 0) | (Settings.Chain ? Role.Chain : 0);

		public List<Round>					Tail = new();
		public Dictionary<int, Round>		LoadedRounds = new();
		//public List<Member>					Members	= new();
		//public List<Account>				Funds = new();

		public RocksDb						Engine;
		public byte[]						BaseState;
		static readonly byte[]				BaseStateKey = new byte[] {0x01};
		//public byte[]						__BaseStateHash;
		static readonly byte[]				__BaseHashKey = new byte[] {0x02};
		public byte[]						BaseHash = Cryptography.ZeroHash;
		static readonly byte[]				ChainStateKey = new byte[] {0x03};
		static readonly byte[]				GenesisKey = new byte[] {0x04};
		public AccountTable					Accounts;
		public AuthorTable					Authors;
		public ProductTable					Products;
		public PlatformTable				Platforms;
		public ReleaseTable					Releases;
		public int							Size => BaseState == null ? 0 : (BaseState.Length + 
																			Accounts.Clusters.Sum(i => i.MainLength) +
																			Authors.Clusters.Sum(i => i.MainLength) +
																			Products.Clusters.Sum(i => i.MainLength) +
																			Platforms.Clusters.Sum(i => i.MainLength) +
																			Releases.Clusters.Sum(i => i.MainLength));
		public Log							Log;
		public BlockDelegate				BlockAdded;

		public Round						LastConfirmedRound;
		public Round						LastCommittedRound;
		public Round						LastNonEmptyRound	=> Tail.FirstOrDefault(i => i.Blocks.Any()) ?? LastConfirmedRound;
		public Round						LastPayloadRound	=> Tail.FirstOrDefault(i => i.Blocks.Any(i => i is Payload)) ?? LastConfirmedRound;

		public const string					ChainFamilyName = "Chain";
		public ColumnFamilyHandle			ChainFamily	=> Engine.GetColumnFamily(ChainFamilyName);

		public static int					GetValidityPeriod(int rid) => rid + Pitch;

		public Database(Zone zone, DatabaseSettings settings, DevSettings dev, SecretSettings secrets, Log log, Vault vault, RocksDb engine)
		{
			Zone = zone;
			Settings = settings;
			Dev = dev;
			Log = log;
			Engine = engine;

			Accounts = new (this);
			Authors = new (this);
			Products = new (this);
			Platforms = new (this);
			Releases = new (this);

			BaseState = Engine.Get(BaseStateKey);

			if(BaseState != null)
			{
				var r = new BinaryReader(new MemoryStream(BaseState));
		
				LastCommittedRound			= new Round(this){Id = r.Read7BitEncodedInt()};
				LastCommittedRound.Hash		= r.ReadSha3();
				LastCommittedRound.Time		= r.ReadTime();
				LastCommittedRound.WeiSpent	= r.ReadBigInteger();
				LastCommittedRound.Factor	= r.ReadCoin();
				LastCommittedRound.Emission	= r.ReadCoin();
				LastCommittedRound.Members	= r.ReadList<Member>();
				LastCommittedRound.Funds	= r.ReadList<AccountAddress>();

				LoadedRounds.Add(LastCommittedRound.Id, LastCommittedRound);

				Hashify();

				if(!BaseHash.SequenceEqual(Engine.Get(__BaseHashKey)))
				{
					throw new IntegrityException("");
				}
			}

			if(Settings.Chain)
			{
				var chainstate = Engine.Get(ChainStateKey);

				if(chainstate == null || !Engine.Get(GenesisKey).SequenceEqual(Zone.Genesis.HexToByteArray()))
				{
					Tail.Clear();

					if(Dev.GenerateGenesis)
					{
						var g = CreateGenesis(secrets);
						
						if(g != Zone.Genesis)
							throw new IntegrityException("Genesis update needed");
						
						Tail.Clear();
					}
	
 					var rd = new BinaryReader(new MemoryStream(Zone.Genesis.HexToByteArray()));
						
					for(int i = 0; i <= LastGenesisRound; i++)
					{
						var r = new Round(this);
						r.Read(rd);
						r.Voted = true;
		
						Tail.Insert(0, r);
				
						if(i == Pitch * 2)
						{
							r.ConfirmedJoiners.Add(new Member {Generator = SecretSettings.Father0, IPs = new [] {Zone.GenesisIP}});
							r.ConfirmedFundJoiners.Add(SecretSettings.Org);
						}
	
						foreach(var p in r.Payloads)
							p.Confirmed = true;
	
						if(r.Id > 0)
							r.Time = CalculateTime(r, r.Unique);
	
						if(i <= LastGenesisRound - Pitch)
							Confirm(r, true);
					}
	
					if(Tail.Any(i => i.Payloads.Any(i => i.Transactions.Any(i => i.Operations.Any(i => i.Error != null)))))
					{
						throw new IntegrityException("Genesis construction failed");
					}
				
					Engine.Put(GenesisKey, Zone.Genesis.HexToByteArray());
				}
				else
				{
					var rd = new BinaryReader(new MemoryStream(chainstate));

					var lcr = FindRound(rd.Read7BitEncodedInt());
					
					for(int i = lcr.Id - lcr.Id % TailLength; i <= lcr.Id; i++)
					{
						var r = FindRound(i);

						Tail.Insert(0, r);
		
						r.Confirmed = false;
						Confirm(r, true);
					}
				}
			}
		}

		public string CreateGenesis(SecretSettings secrets)
		{
			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = FindRound(rid);
				r.ConfirmedPayloads = r.Payloads.ToList();
				r.Hashify(r.Id > 0 ? FindRound(rid - 1).Hash : Cryptography.ZeroHash);
				r.Write(w);
			}
	
			var jr = new JoinMembersRequest(this)
						{
							RoundId	= Pitch,
							IPs		= new [] {Zone.GenesisIP}
						};
// 
 			jr.Sign(secrets.Fathers[0]);
// 						jr.Write(w);

			var b0 = new Payload(this)
						{
							RoundId		= 0,
							TimeDelta	= 1,
							Consensus	= Consensus.Empty,
						};

			var t = new Transaction(Zone, secrets.OrgAccount);
			t.AddOperation(new Emission(secrets.OrgAccount, Web3.Convert.ToWei(512_000, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
			t.AddOperation(new AuthorBid(secrets.OrgAccount, "uo", 1){ Id = 1 });
			t.Sign(secrets.GenAccount, 0);
			b0.AddNext(t);
						
			void emmit(Dictionary<AccountAddress, AccountEntry> accs)
			{
				foreach(var f in secrets.Fathers)
				{
					var t = new Transaction(Zone, f);
					t.AddOperation(new Emission(f, Web3.Convert.ToWei(1000, UnitConversion.EthUnit.Ether), 0){ Id = 0 });
									
					if(accs != null)
					{
						t.AddOperation(new CandidacyDeclaration(f, accs[f].Balance - 1){ Id = 1 });
					}

					t.Sign(secrets.GenAccount, 0);
	
					b0.AddNext(t);
				}

				b0.Sign(secrets.GenAccount);
				Add(b0);
			}
						
			emmit(null);

			Execute(Tail.First(), Tail.First().Payloads, null);
			var accs = Tail.First().AffectedAccounts;
			Tail.Clear();
			b0.Transactions.Clear();
			b0.AddNext(t);

			emmit(accs);

			b0.FundJoiners.Add(secrets.OrgAccount);
	
			write(0);
						
			for(int i = 1; i < Pitch; i++)
			{
				var b = new Payload(this)
						{
							RoundId		= i,
							TimeDelta	= i == 1 ? ((long)TimeSpan.FromDays(365).TotalMilliseconds + 1) : 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
							Consensus	= Consensus.Empty,
						};
	
				if(i == 1)
				{
					t = new Transaction(Zone, secrets.OrgAccount);
					t.AddOperation(new AuthorRegistration(secrets.OrgAccount, "uo", "UO", 255){ Id = 2 });
					t.Sign(secrets.GenAccount, i);
					b.AddNext(t);
				}
								
				b.Sign(secrets.GenAccount);
				Add(b);
	
				write(i);
			}
	
			Add(jr);

			for(int i = Pitch; i <= LastGenesisRound; i++)
			{
				var p = GetRound(i - Pitch);
	
				var b = new Vote(this)
						{
							RoundId		= i,
							TimeDelta	= 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
							Consensus	= ProposeConsensus(p)
						};

				if(i == jr.RoundId)
				{
					Add(jr);
				}
		
				if(i == Pitch * 2)
					b.Joiners.Add(secrets.Fathers[0]);
	
				b.Sign(secrets.GenAccount);
				Add(b);
	
				if(i > LastGenesisRound - Pitch)
				{
					var v = new Vote(this)
							{
								RoundId		= i,
								TimeDelta	= 1,  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
								Consensus	= ProposeConsensus(p)
							};
	
					v.Sign(secrets.Fathers[0]);
					Add(v);
				}

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		public void Add(Block b)
		{
			var r = GetRound(b.RoundId);

			b.Round = r;

			r.Blocks.Add(b);
			r.Blocks = r.Blocks.OrderBy(i => i is Payload p ? p.OrderingKey : new byte[] {}, new BytesComparer()).ToList();
	
			if(b is Payload p)
			{
				foreach(var t in p.Transactions)
					foreach(var o in t.Operations)
						o.Placing = PlacingStage.Placed;

				//if(execute)
	 			//	for(int i = r.Id; i <= LastPayloadRound.Id; i++)
	 			//	{
	 			//		var ir = GetRound(i);
	 			//			
	 			//		if(ir.Payloads.Any() && ir.Previous != null)
				//		{
				//			ir.Time = CalculateTime(ir, ir.Unique);
				//			Execute(ir, ir.Payloads, null);
				//		}
	 			//		else
	 			//			break;
	 			//	}
			}
	
			if(r.FirstArrivalTime == DateTime.MaxValue)
			{
				r.FirstArrivalTime = DateTime.UtcNow;
			} 


// 			if(b is MembersJoinRequest jr)
// 			{
// 				//jr.Bail = Accounts.Find(jr.Generator, b.RoundId - Pitch).Bail;
// 				JoinRequests.Add(jr);
// 			}
	
			BlockAdded?.Invoke(b);
		}

		public void Add(IEnumerable<Block> bb)
		{
			foreach(var i in bb)
			{
				Add(i);
			}
		}

		public Round GetRound(int rid)
		{
			var r = FindRound(rid);

			if(r == null)
			{	
				r = new Round(this) {Id = rid};
				//r.LastAccessed = DateTime.UtcNow;
				Tail.Add(r);
				Tail = Tail.OrderByDescending(i => i.Id).ToList();
			}

			return r;
		}

		public Round FindRound(int rid)
		{
			foreach(var i in Tail)
				if(i.Id == rid)
				{
					//i.LastAccessed = DateTime.UtcNow;
					return i;
				}

			if(LoadedRounds.ContainsKey(rid))
			{
				var r = LoadedRounds[rid];
				//r.LastAccessed = DateTime.UtcNow;
				return r;
			}

			var d = Engine.Get(BitConverter.GetBytes(rid), ChainFamily);

			if(d != null)
			{
				var r = new Round(this);
				r.Id			= rid; 
				r.Voted			= true; 
				r.Confirmed		= true;
				//r.LastAccessed	= DateTime.UtcNow;

				r.Load(new BinaryReader(new MemoryStream(d)));
	
				LoadedRounds[r.Id] = r;
				//Recycle();
				
				return r;
			}
			else
				return null;
		}

		void Recycle()
		{
			if(LoadedRounds.Count > TailLength * 10)
			{
				foreach(var i in LoadedRounds.OrderByDescending(i => i.Value.Id).Skip(TailLength * 10))
				{
					LoadedRounds.Remove(i.Key);
				}
			}
		}

		public IEnumerable<Member> VoterOf(Round r)
		{
			return FindRound(r.Id - Pitch - 1).Members/*.Where(i => i.JoinedAt < r.Id)*/;
		}

		public bool QuorumReached(Round r)
		{
			var members = VoterOf(r).Select(i => i.Generator);

			var n = r.Majority.Count(i => members.Contains(i.Generator));
			
			var q = members.Count() * 2 / 3;

			if(members.Count() * 2 % 3 != 0)
				q++;

			return q <= n;
		}

		public bool QuorumFailed(Round r)
		{
			var max = VoterOf(r).Select(i => i.Generator);

			return r.Unique.Count() >= Math.Max(1, max.Count() * 2/3) && r.Majority.Count() + (max.Count() - r.Unique.Count()) < Math.Max(1, max.Count() * 2/3);
		}

		public ChainTime CalculateTime(Round round, IEnumerable<Vote> votes)
		{
 			if(round.Id == 0)
 			{
 				return round.Time;
 			}

 			if(!votes.Any())
 			{
				return round.Previous.Time + new ChainTime(1);
			}

			///var t = 0L;
			///var n = 0;
			///
			///for(int i = Math.Max(0, round.Id - Pitch + 1); i <= round.Id; i++)
			///{
			///	var r = FindRound(i);
			///	t += r.Payloads.Sum(i => i.Time.Ticks);
			///	n += r.Payloads.Count();
			///}
			///
			///t /= n;

			votes = votes.OrderBy(i => i.Generator);

			return round.Previous.Time + new ChainTime(votes.Sum(i => i.TimeDelta)/votes.Count());
		}

		public IEnumerable<Transaction> CollectValidTransactions(IEnumerable<Transaction> txs, Round round)
		{
			//txs = txs.Where(i => round.Id <= i.RoundMax /*&& IsSequential(i, round.Id)*/);

			if(txs.Any())
			{
// 				var p = new Payload(this);
// 				p.Member	= Account.Zero;
// 				p.Time		= DateTime.UtcNow;
// 				p.Round		= round;
// 				p.TimeDelta	= 1;
// 					
// 				foreach(var i in txs)
// 				{
// 					p.AddNext(i);
// 				}
// 				
//  				Execute(round, new Payload[] {p}, null);
 	
 //				txs = txs.Where(t => t.SuccessfulOperations.Any());
			}

			return txs;
		}

		///public bool IsSequential(Operation transaction, int ridmax)
		///{
		///	var prev = Accounts.FindLastOperation(transaction.Signer, o => o.Successful, t => t.Successful, null, r => r.Id < ridmax);
		///
		///	if(transaction.Id == 0 && prev == null)
		///		return true;
		///
		///	if(transaction.Id == 0 && prev != null || transaction.Id != 0 && prev != null && prev.Id != transaction.Id - 1)
		///		return false;
		///
		///	/// STRICT: return prev != null && (prev.Payload.Confirmed || prev.Payload.Transactions.All(i => IsSequential(i, i.Payload.RoundId))); /// All transactions in a block containing 'prev' one must also be sequential
		///	return prev.Transaction.Payload.Confirmed || IsSequential(prev, prev.Transaction.Payload.RoundId);
		///}

		public IEnumerable<AccountAddress> ProposeJoiners(Round round)
		{
			var o = round.Parent.JoinRequests.Select(jr =>	{
																var a = Accounts.Find(jr.Generator, LastConfirmedRound.Id);
																return new {jr = jr, a = a};
															})	/// round.ParentId - Pitch means to not join earlier than [Pitch] after declaration, and not redeclare after a join is requested
											.Where(i => i.a != null && 
														i.a.CandidacyDeclarationRid <= round.Id - Pitch * 2 &&  /// 2 = declared, requested
														i.a.Bail >= (Dev != null && Dev.DisableBailMin ? 0 : BailMin))
											.OrderByDescending(i => i.a.Bail)
											.ThenBy(i => i.a.Address)
											.Select(i => i.jr);

			var n = Math.Min(MembersMax - VoterOf(round).Count(), o.Count());

			return o.Take(n).Select(i => i.Generator);
		}

		public IEnumerable<AccountAddress> ProposeLeavers(Round round, AccountAddress generator)
		{
			var joiners = ProposeJoiners(round);

			var leavers = VoterOf(round).Where(i =>	i.JoinedAt < round.ParentId &&
													Tail.Count(r =>	round.ParentId <= r.Id && r.Id < round.Id &&					/// in previous Pitch number of rounds
																	r.Votes.Any(b => b.Generator == i.Generator)) < Pitch * 2/3 &&	/// sent less than 2/3 of required blocks
													!Enumerable.Range(round.ParentId + 1, Pitch - 1).Select(i => FindRound(i)).Any(r => r.Votes.Any(v => v.Generator == generator && v.Leavers.Contains(i.Generator)))) /// not yet reported in prev [Pitch-1] rounds
										.Select(i => i.Generator);

			return leavers;
		}

		public Consensus ProposeConsensus(Round round)
		{
			//if(round.Id < Pitch)
			//	return RoundReference.Empty;

			if(round.Id > LastGenesisRound && !round.Parent.Confirmed)
				return null;

			//if(!round.Payloads.Any())
			//	return null;

			var payloads = round.Majority.OfType<Payload>();

			round.Time = CalculateTime(round, payloads);
			Execute(round, payloads, round.Forkers);

			payloads = payloads.Where(i => i.SuccessfulTransactions.Any()).OrderBy(i => i.OrderingKey, new BytesComparer());
			//var nonempties = choice.Where(i => i.SuccessfulTransactions.Any());

			/// take only blocks with valid transactions or take first empty block 
			///var pp = (payloads);
			
			var rr = new Consensus();

			rr.Parent		= (round.Id >= Pitch ? round.Parent.Hash : Cryptography.ZeroHash);
			rr.Payloads		= payloads.					Select(i => i.Prefix).ToList();
			rr.Joiners		= round.ElectedJoiners.		Select(i => i.Prefix).OrderBy(i => i, new BytesComparer())/*.Take(rr.Leavers.Count + NewMembersPerRoundMax)*/.ToList();
			rr.Leavers		= round.ElectedLeavers.		Select(i => i.Prefix).OrderBy(i => i, new BytesComparer()).ToList();
			rr.Violators	= round.ElectedViolators.	Select(i => i.Prefix).OrderBy(i => i, new BytesComparer()).ToList();
			rr.FundLeavers	= round.ElectedFundLeavers.	Select(i => i.Prefix).OrderBy(i => i, new BytesComparer()).ToList();
			rr.FundJoiners	= round.ElectedFundJoiners.	Select(i => i.Prefix).OrderBy(i => i, new BytesComparer()).ToList();
			rr.Time			= CalculateTime(round, round.Unique);

			return rr; 
		}

		public void Execute(Round round, IEnumerable<Payload> payloads, IEnumerable<AccountAddress> forkers)
		{
			var prev = round.Previous;
				
			if(round.Id != 0 && prev == null)
				return;

			foreach(var b in payloads)
				foreach(var t in b.Transactions)
					foreach(var o in t.Operations)
					{
						//o.Successful = false;
						o.Error = null;
					}

			start: 

			round.Emission	= round.Id == 0 ? 0						: prev.Emission;
			round.WeiSpent	= round.Id == 0 ? 0						: prev.WeiSpent;
			round.Factor	= round.Id == 0 ? Emission.FactorStart	: prev.Factor;
			round.Members	= round.Id == 0 ? new()					: prev.Members.ToList();
			round.Funds		= round.Id == 0 ? new()					: prev.Funds.ToList();

			round.AffectedAccounts.Clear();
			round.AffectedAuthors.Clear();
			round.AffectedProducts.Clear();
			round.AffectedPlatforms.Clear();
			round.AffectedReleases.Clear();

			foreach(var b in payloads.AsEnumerable().Reverse())
			{
				foreach(var t in b.Transactions.AsEnumerable().Reverse())
				{
					if(t.Operations.Any(i => i.Error != null))
						continue;

					Coin fee = 0;

					foreach(var o in t.Operations.AsEnumerable().Reverse())
					{
						//if(o.Error != null)
						//	continue;

						var s = round.AffectAccount(t.Signer);
					
						if(o.Id <= s.LastOperationId)
						{
							o.Error = Operation.NotSequential;
							goto start;
						}
						
						o.Execute(this, round);

						if(o.Error != null)
							goto start;

						var f = o.CalculateFee(round.Factor);
	
						if(s.Balance - f < 0)
						{
							o.Error = Operation.NotEnoughUNT;
							goto start;
						}

						fee += f;
						s.Balance -= f;
						s.LastOperationId = o.Id;
					}
						
					if(t.SuccessfulOperations.Count() == t.Operations.Count)
					{
						if(Settings.Chain)
						{
							round.AffectAccount(t.Signer).Transactions.Add(round.Id);
						}

						round.Distribute(fee, new[]{b.Generator}, 9, round.Funds, 1); /// taking 10% we prevent a member from sending his own transactions using his own blocks for free, this could be used for block flooding
					}
				}
			}

			if(round.Id > LastGenesisRound)
			{
				var penalty = Coin.Zero;

				if(forkers != null && forkers.Any())
				{
					foreach(var f in forkers)
					{
						penalty += round.AffectAccount(f).Bail;
						round.AffectAccount(f).BailStatus = BailStatus.Siezed;
					}

					round.Distribute(penalty, round.Members.Where(i => !forkers.Contains(i.Generator)).Select(i => i.Generator), 1, round.Funds, 1);
				}
			}
		}

		public void Hashify()
		{
			BaseHash = Zone.Cryptography.Hash(BaseState);
	
			foreach(var i in Accounts.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Authors.SuperClusters.OrderBy(i => i.Key))			BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Products.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Platforms.SuperClusters.OrderBy(i => i.Key))	BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
			foreach(var i in Releases.SuperClusters.OrderBy(i => i.Key))		BaseHash = Zone.Cryptography.Hash(Bytes.Xor(BaseHash, i.Value));
		}

		public void Confirm(Round round, bool confirmed)
		{
			if(round.Id > 0 && LastConfirmedRound != null && LastConfirmedRound.Id + 1 != round.Id)
				throw new IntegrityException("LastConfirmedRound.Id + 1 == round.Id");

			if(!confirmed)
			{
				List<T>	confirm<T>(IEnumerable<byte[]> prefixes, Func<Vote, IEnumerable<T>> get, Func<T, byte[]> getprefix)
				{
					var o = prefixes.Select(v => round.Unique.SelectMany(i => get(i)).FirstOrDefault(i => getprefix(i).SequenceEqual(v)));
	
					if(o.Contains(default(T)))
						throw new ConfirmationException("Can't confirm, some references not found", round);
					else 
						return o.ToList();
				}
	
				/// check we have all payload blocks 
	
				foreach(var i in round.Payloads)
					i.Confirmed = false;
	
				var child = FindRound(round.Id + Pitch);
				var c = child.Majority.First().Consensus;
	 	
 				foreach(var pf in c.Payloads)
 				{
 					var b = round.Unique.OfType<Payload>().FirstOrDefault(i => pf.SequenceEqual(i.Prefix));
 	
 					if(b != null)
 						b.Confirmed = true;
 					else
 						return;
 				}

				round.Time					= c.Time;
				round.ConfirmedPayloads		= round.Payloads.Where(i => i.Confirmed).OrderBy(i => i.OrderingKey, new BytesComparer()).ToList();
				round.ConfirmedJoiners		= confirm(c.Joiners,		i => i.Joiners,		i => i.Prefix).Select(i => new Member{Generator = i, IPs = round.Parent.JoinRequests.First(q => q.Generator == i).IPs}).ToList();
				round.ConfirmedLeavers		= confirm(c.Leavers,		i => i.Leavers,		i => i.Prefix);
				round.ConfirmedViolators	= confirm(c.Violators,		i => i.Violators,	i => i.Prefix);
				round.ConfirmedFundJoiners	= confirm(c.FundJoiners,	i => i.FundJoiners, i => i.Prefix);
				round.ConfirmedFundLeavers	= confirm(c.FundLeavers,	i => i.FundLeavers, i => i.Prefix);
			}
			else
				round.ConfirmedPayloads		= round.Payloads.ToList();

			Execute(round, round.ConfirmedPayloads, round.ConfirmedViolators);
			
			foreach(var b in round.Payloads)
			{
				foreach(var t in b.Transactions)
				{	
					foreach(var o in t.Operations)
					{	
						o.Placing = (b.Confirmed && o.Error == null) ? PlacingStage.Confirmed : PlacingStage.FailedOrNotFound;
#if DEBUG
						if(o.__ExpectedPlacing > PlacingStage.Placed && o.Placing != o.__ExpectedPlacing)
						{
							Debugger.Break();
						}
#endif
					}

					t.Operations.RemoveAll(i => i.Error != null);
				}

				b.Transactions.RemoveAll(t => !t.Operations.Any());
			}

			round.Members.AddRange(round.ConfirmedJoiners	.Where(i => Accounts.Find(i.Generator, round.Id).CandidacyDeclarationRid <= round.Id - Pitch * 2)
															.Select(i => new Member {Generator = i.Generator, IPs = i.IPs, JoinedAt = round.Id + Pitch}));
	
			round.Members.RemoveAll(i => round.AnyOperation(o => o is CandidacyDeclaration d && d.Signer == i.Generator && o.Placing == PlacingStage.Confirmed));  /// CandidacyDeclaration cancels membership
			round.Members.RemoveAll(i => round.AffectedAccounts.ContainsKey(i.Generator) && round.AffectedAccounts[i.Generator].Bail < (Dev.DisableBailMin ? 0 : BailMin));  /// if Bail has exhausted due to penalties (CURRENTY NOT APPLICABLE, penalties are disabled)
			round.Members.RemoveAll(i => round.ConfirmedLeavers.Contains(i.Generator));
			round.Members.RemoveAll(i => round.ConfirmedViolators.Contains(i.Generator));
	
			//if(round.Id <= LastGenesisRound || round.Factor == Emission.FactorEnd) /// Funds reorganization only after emission is over
			{
				round.Funds.AddRange(round.ConfirmedFundJoiners);
				round.Funds.RemoveAll(i => round.ConfirmedFundLeavers.Contains(i));
			}

			using(var b = new WriteBatch())
			{
				if(Tail.Count(i => i.Id < round.Id) >= TailLength)
				{
					var tail = Tail.AsEnumerable().Reverse().Take(TailLength);
	
					foreach(var i in tail)
					{
						Accounts	.Save(b, i.AffectedAccounts.Values);
						Authors		.Save(b, i.AffectedAuthors.Values);
						Products	.Save(b, i.AffectedProducts.Values);
						Platforms	.Save(b, i.AffectedPlatforms.Values);
						Releases	.Save(b, i.AffectedReleases.Values);
					}

					LastCommittedRound = tail.Last();
	
					var s = new MemoryStream();
					var w = new BinaryWriter(s);
	
					w.Write7BitEncodedInt(LastCommittedRound.Id);
					w.Write(LastCommittedRound.Hash);
					w.Write(LastCommittedRound.Time);
					w.Write(LastCommittedRound.WeiSpent);
					w.Write(LastCommittedRound.Factor);
					w.Write(LastCommittedRound.Emission);
					w.Write(LastCommittedRound.Members.OrderBy(i => i.Generator));
					w.Write(LastCommittedRound.Funds.OrderBy(i => i));
	
					BaseState = s.ToArray();

					Hashify();
									
					b.Put(BaseStateKey, BaseState);
					b.Put(__BaseHashKey, BaseHash);

					foreach(var i in tail)
					{
						if(!LoadedRounds.ContainsKey(i.Id))
						{
							LoadedRounds.Add(i.Id, i);
						}
						
						Tail.Remove(i);
					}

					Recycle();
				}

				round.Hashify(round.Id > 0 ? round.Previous.Hash : Cryptography.ZeroHash); /// depends on BaseHash 

				if(Settings.Chain)
				{
					var s = new MemoryStream();
					var w = new BinaryWriter(s);

					w.Write7BitEncodedInt(round.Id);

					b.Put(ChainStateKey, s.ToArray());

					s = new MemoryStream();
					w = new BinaryWriter(s);
	
					round.Save(w);
	
					b.Put(BitConverter.GetBytes(round.Id), s.ToArray(), ChainFamily);
				}

				Engine.Write(b);
			}

			if(round.Id > Pitch)
			{
				var cjr = FindRound(round.Id - Pitch - 1);
				
				if(cjr != null)
				{
					cjr.Blocks.RemoveAll(i => i is not Payload);
				}
			}

			//round.JoinRequests.RemoveAll(i => i.RoundId < round.Id - Pitch);
		
			round.Confirmed = true;
			LastConfirmedRound = round;
		}
/*
		public ProductEntry FindProduct(ProductAddress product, int ridmax)
		{
			if(ridmax >= LastCommittedRound.Id)
				throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Tail.Where(i => i.Id <= ridmax))
				if(r.AffectedProducts.ContainsKey(product))
					return r.AffectedProducts[product];

			var e = Products.FindEntry(product);

			return e;
		}
*/
		public Transaction FindLastTailTransaction(Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in b.Transactions)
						if(transaction_predicate == null || transaction_predicate(t))
							return t;

			return null;
		}

		public IEnumerable<Transaction> FindLastTailTransactions(Func<Transaction, bool> transaction_predicate, Func<Payload, bool> payload_predicate = null, Func<Round, bool> round_predicate = null)
		{
			foreach(var r in round_predicate == null ? Tail : Tail.Where(round_predicate))
				foreach(var b in payload_predicate == null ? r.Payloads : r.Payloads.Where(payload_predicate))
					foreach(var t in transaction_predicate == null ? b.Transactions : b.Transactions.Where(transaction_predicate))
						yield return t;
		}

		public O FindLastTailOperation<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops.FirstOrDefault() : ops.FirstOrDefault(op);
		}

		IEnumerable<O> FindLastTailOperations<O>(Func<O, bool> op = null, Func<Transaction, bool> tp = null, Func<Payload, bool> pp = null, Func<Round, bool> rp = null)
		{
			var ops = FindLastTailTransactions(tp, pp, rp).SelectMany(i => i.Operations.OfType<O>());
			return op == null ? ops : ops.Where(op);
		}

		public Block FindLastBlock(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			for(int i = LastNonEmptyRound.Id; i >= maxrid; i--)
			{
				var r = FindRound(i);

				if(r != null)
				{
					foreach(var b in r.Blocks)
						if(f(b))
							return b;
				}
			}
			//foreach(var r in Rounds.Where(i => i.Id <= maxrid))
			//	foreach(var b in r.Blocks)
			//		if(f(b))
			//			return b;

			return null;
		}

		public IEnumerable<Block> FindLastBlocks(Func<Block, bool> f, int maxrid = int.MaxValue)
		{
			foreach(var r in Tail.Where(i => i.Id <= maxrid))
				foreach(var b in r.Blocks)
					if(f(b))
						yield return b;
		}


		public Release QueryRelease(ReleaseQuery query)
		{
			if(query.VersionQuery == VersionQuery.Latest)
			{
				var p = Products.Find(query.Realization.Product, LastConfirmedRound.Id);
	
				if(p != null)
				{
					//var r = p.Releases.Where(i => i.Realization == query.Platform && i.Channel == query.Channel).MaxBy(i => i.Version);
					//
					//if(r != null)
					//{
					//	var rr = FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	(RealizationAddress)m.Release == query.Realization && 
					//																		m.Release.Version == r.Version);
					//
					//	return new QueryReleaseResult{Registration = rr};
					//}

					return Releases.Where(	query.Realization.Product.Author, 
											query.Realization.Product.Name, 
											i => i.Address.Platform == query.Realization.Platform, 
											LastConfirmedRound.Id)
									.MaxBy(i => i.Address.Version);

				}

				return null;
			}

			if(query.VersionQuery == VersionQuery.Exact)
			{
				var p = Products.Find(query.Realization.Product, LastConfirmedRound.Id);
	
				if(p != null)
				{
					//var r = p.Releases.Where(i => i.Realization == query.Platform && i.Channel == query.Channel).MaxBy(i => i.Version);
					//
					//if(r != null)
					//{
					//	var rr = FindRound(r.Rid).FindOperation<ReleaseRegistration>(m =>	(RealizationAddress)m.Release == query.Realization && 
					//																		m.Release.Version == r.Version);
					//
					//	return new QueryReleaseResult{Registration = rr};
					//}

					var r = query.Realization;

					return Releases.Find(new ReleaseAddress(r.Product, r.Platform, query.Version), LastConfirmedRound.Id);
				}

				return null;
			}

			throw new ArgumentException("Unsupported VersionQuery");
		}
	}
}
