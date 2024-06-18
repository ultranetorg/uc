using System;
using System.Collections.Generic;
using System.Linq;
using RocksDbSharp;

namespace Uccs.Net
{
	public abstract class RdnCall<R> : McvCall<R> where R : PeerResponse
	{
		public new Rdn	Node => base.Node as Rdn;
		public RdnMcv			Rdn => Node.Mcv;
	}

	[Flags]
	public enum RdnRole : uint
	{
		None,
		Seed		= 0b00000100,
	}

	public class RdnMcv : Mcv
	{
		public new RdnSettings  Settings => base.Settings as RdnSettings;
		public DomainTable		Domains;

		public List<ForeignResult>		ApprovedEmissions = new();
		public List<ForeignResult>		ApprovedMigrations = new();

		public RdnMcv(McvZone zone, RdnSettings settings, string databasepath, bool skipinitload = false) : base(zone, settings, databasepath, skipinitload)
		{
		}

		public RdnMcv(Rdn sun, RdnSettings settings, string databasepath, Flow flow, IEthereum nas, IClock clock) : base(sun, settings, databasepath, clock, flow)
		{
		}

		protected override void GenesisCreate(Vote vote)
		{
			(vote as RdnVote).Emissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
		}

		protected override void GenesisInitilize(Round round)
		{
			if(round.Id == 1)
				(round as RdnRound).ConsensusEmissions = [new ForeignResult {OperationId = new(0, 0, 0), Approved = true}];
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

  			v.Emissions		= ApprovedEmissions.ToArray();
			v.Migrations	= ApprovedMigrations.ToArray();
		}

	}
}
