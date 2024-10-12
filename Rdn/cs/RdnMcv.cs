using System.Net;
using RocksDbSharp;

namespace Uccs.Rdn
{
	public abstract class RdnPpc<R> : McvPpc<R> where R : PeerResponse
	{
		public new RdnTcpPeering	Peering => base.Peering as RdnTcpPeering;
		public new RdnNode			Node => base.Node as RdnNode;
		public new RdnMcv			Mcv => base.Mcv as RdnMcv;
	}

	[Flags]
	public enum RdnRole : uint
	{
		None,
		Seed = 0b00000100,
	}

	public class RdnMcv : Mcv
	{
		public DomainTable				Domains;
		//public List<ForeignResult>	ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();
		IPAddress[]						BaseIPs;
		IPAddress[]						SeedHubIPs;

		public RdnMcv()
		{
  		}

		public RdnMcv(Rdn net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
		{
		}

		public RdnMcv(Rdn sun, McvSettings settings, string databasepath, IPAddress[] baseips, IPAddress[] seedhubips, IClock clock) : base(sun, settings, databasepath, clock)
		{
			BaseIPs = baseips;
			SeedHubIPs = seedhubips;
		}

		public override string CreateGenesis(AccountKey god, AccountKey f0)
		{
			/// 0	- declare F0
			/// P	- confirmed F0 membership
			/// P+P	- F0 start voting for P+P-P-1 = P-1

			Clear();

			var s = new MemoryStream();
			var w = new BinaryWriter(s);

			void write(int rid)
			{
				var r = GetRound(rid);
				r.ConsensusTransactions = r.OrderedTransactions.ToArray();
				r.Hashify();
				r.Write(w);
			}
	
			var v0 = CreateVote(); 
			{
				v0.RoundId = 0;
				v0.Time = Time.Zero;
				v0.ParentHash = Net.Cryptography.ZeroHash;

				var t = new Transaction {Net = Net, Nid = 0, Expiration = 0};
				t.Generator = new([0, 0], -1);
				t.AddOperation(new UtilityTransfer(f0, Net.ECDayEmission, Net.ECLifetime, Net.BYDayEmission));
				t.Sign(god, Net.Cryptography.ZeroHash);
				v0.AddTransaction(t);

				t = new Transaction {Net = Net, Nid = 0, Expiration = 0};
				t.Generator = new([0, 0], -1);
				t.AddOperation(new RdnCandidacyDeclaration {BaseRdcIPs = [Net.Father0IP], SeedHubRdcIPs = [Net.Father0IP] });
				t.Sign(f0, Net.Cryptography.ZeroHash);
				v0.AddTransaction(t);
			
				v0.Sign(god);
				Add(v0);
				///v0.FundJoiners = v0.FundJoiners.Append(Net.Father0).ToArray();
				write(0);
			}
	
			for(int i = 1; i <= LastGenesisRound; i++)
			{
				var v = CreateVote();
				v.RoundId	 = i;
				v.Time		 = Time.Zero;  //new AdmsTime(AdmsTime.FromYears(datebase + i).Ticks + 1),
				v.ParentHash = i < P ? Net.Cryptography.ZeroHash : GetRound(i - P).Summarize();
		
				v.Sign(i < JoinToVote ? god : f0);
				Add(v);

				write(i);
			}
						
			return s.ToArray().ToHex();
		}

		protected override void GenesisCreate(Vote vote)
		{
			//(vote as RdnVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			#if IMMISSION
			if(round.Id == 1)
				(round as RdnRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
			#endif
		}

		protected override void CreateTables(string databasepath)
		{
			var dbo	= new DbOptions().SetCreateIfMissing(true)
									 .SetCreateMissingColumnFamilies(true);

			var cfs = new ColumnFamilies();
			
			foreach(var i in new ColumnFamilies.Descriptor[]{	new (AccountTable.MetaColumnName,	new ()),
																new (AccountTable.MainColumnName,	new ()),
																new (AccountTable.MoreColumnName,	new ()),
																new (DomainTable.MetaColumnName,	new ()),
																new (DomainTable.MainColumnName,	new ()),
																new (DomainTable.MoreColumnName,	new ()),
																new (ChainFamilyName,				new ())})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Domains = new (this);

			Tables = [Accounts, Domains];
		}

		public override Round CreateRound()
		{
			return new RdnRound(this);
		}

		public override Vote CreateVote()
		{
			return new RdnVote(this);
		}

		public override Generator CreateGenerator()
		{
			return new RdnGenerator();
		}

		public override CandidacyDeclaration CreateCandidacyDeclaration()
		{
			return new RdnCandidacyDeclaration {BaseRdcIPs		= BaseIPs,
												SeedHubRdcIPs	= SeedHubIPs};

		}

		public override void ClearTables()
		{
			Domains.Clear();
		}

		public IEnumerable<Resource> QueryResource(string query)
		{
			var r = Ura.Parse(query);
		
			var a = Domains.Find(r.Domain, LastConfirmedRound.Id);

			if(a == null)
				yield break;

			foreach(var i in a.Resources.Where(i => i.Address.Resource.StartsWith(r.Resource)))
				yield return i;
		}

		public override void FillVote(Vote vote)
		{
			var v = vote as RdnVote;

  			//v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

	}
}
