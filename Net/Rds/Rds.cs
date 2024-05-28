using System;
using System.Collections.Generic;
using System.Linq;
using RocksDbSharp;

namespace Uccs.Net
{
	public class Rds : Mcv
	{
		public DomainTable				Domains;
		public override Guid			Guid => new Guid("A8B619CB-8A8C-4C71-847A-4A182ABDE2B9");

		public Rds(Zone zone, Role roles, McvSettings settings, string databasepath) : base(zone, roles, settings, databasepath)
		{
		}

		public Rds(Zone zone, string databasepath) : base(zone, databasepath)
		{
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
			return new RdsRound(this);
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

	}
}
