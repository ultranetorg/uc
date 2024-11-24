using System.Net;
using RocksDbSharp;

namespace Uccs.Fair
{
	public class FairMcv : Mcv
	{
		public PublisherTable				Publishers;
		//public List<ForeignResult>	ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();
		IPAddress[]						BaseIPs;
		IPAddress[]						SeedHubIPs;

		public FairMcv()
		{
  		}

		public FairMcv(Fair net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
		{
		}

		public FairMcv(Fair sun, McvSettings settings, string databasepath, IPAddress[] baseips, IPAddress[] seedhubips, IClock clock) : base(sun, settings, databasepath, clock)
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
				t.Member = new([0, 0], -1);
				t.AddOperation(new UtilityTransfer(f0, Net.ECDayEmission, Net.ECLifetime, Net.BYDayEmission));
				t.Sign(god, Net.Cryptography.ZeroHash);
				v0.AddTransaction(t);

				t = new Transaction {Net = Net, Nid = 0, Expiration = 0};
				t.Member = new([0, 0], -1);
				t.AddOperation(new CandidacyDeclaration {BaseRdcIPs = [Net.Father0IP]});
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
			//(vote as FairVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			#if IMMISSION
			if(round.Id == 1)
				(round as FairRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
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
																new (PublisherTable.MetaColumnName,	new ()),
																new (PublisherTable.MainColumnName,	new ()),
																new (PublisherTable.MoreColumnName,	new ()),
																new (ChainFamilyName,				new ())})
				cfs.Add(i);

			Database = RocksDb.Open(dbo, databasepath, cfs);

			Accounts = new (this);
			Publishers = new (this);

			Tables = [Accounts, Publishers];
		}

		public override Round CreateRound()
		{
			return new FairRound(this);
		}

		public override Vote CreateVote()
		{
			return new Vote(this);
		}

		public override Generator CreateGenerator()
		{
			return new Generator();
		}

		public override CandidacyDeclaration CreateCandidacyDeclaration()
		{
			return new CandidacyDeclaration {BaseRdcIPs	= BaseIPs};
		}

		public override void ClearTables()
		{
			Publishers.Clear();
		}

		public override void FillVote(Vote vote)
		{
  			//v.Emissions		= ApprovedEmissions.ToArray();
			//v.Migrations	= ApprovedMigrations.ToArray();
		}
	}
}
