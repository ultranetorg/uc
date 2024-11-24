using System.Net;
using RocksDbSharp;

namespace Uccs.Fair
{
	public class FairMcv : Mcv
	{
		public PublisherTable			Publishers;
		IPAddress[]						BaseIPs;

		public FairMcv()
		{
  		}

		public FairMcv(Fair net, McvSettings settings, string databasepath, bool skipinitload = false) : base(net, settings, databasepath, skipinitload)
		{
		}

		public FairMcv(Fair sun, McvSettings settings, string databasepath, IPAddress[] baseips, IClock clock) : base(sun, settings, databasepath, clock)
		{
			BaseIPs = baseips;
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
		}
	}
}
