using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Uccs.Net
{
	public class RdsRound : Round
	{
		public List<DomainMigration>			Migrations;
		public Rds								Rds => Mcv as Rds;
		public Dictionary<string, DomainEntry>	AffectedDomains = new();
		public ForeignResult[]					ConsensusMigrations = {};

		public RdsRound(Rds rds) : base(rds)
		{
		}

		public override IEnumerable<object> AffectedByTable(TableBase table)
		{
			if(table == Mcv.Accounts)
				return AffectedAccounts.Values;

			if(table == Rds.Domains)
				return AffectedDomains.Values;

			throw new IntegrityException();
		}

		public DomainEntry AffectDomain(string domain)
		{
			if(AffectedDomains.TryGetValue(domain, out DomainEntry a))
				return a;
			
			var e = Rds.Domains.Find(domain, Id - 1);

			if(e != null)
			{
				AffectedDomains[domain] = e.Clone();
				AffectedDomains[domain].Affected  = true;;
				return AffectedDomains[domain];
			}
			else
			{
				var ci = Rds.Domains.KeyToCluster(domain).ToArray();
				var c = Rds.Domains.Clusters.FirstOrDefault(i => i.Id.SequenceEqual(ci));

				int ai;
				
				if(c == null)
					NextDomainIds[ci] = 0;
				else
					NextDomainIds[ci] = c.NextEntityId;
				
				ai = NextDomainIds[ci]++;

				return AffectedDomains[domain] = new DomainEntry(Mcv){	Affected = true,
																		New = true,
																		Id = new EntityId(ci, ai), 
																		Address = domain};
			}
		}

		public DomainEntry AffectDomain(EntityId id)
		{
			var a = AffectedDomains.Values.FirstOrDefault(i => i.Id == id);
			
			if(a != null)
				return a;
			
			a = Rds.Domains.Find(id, Id - 1);

			if(a == null)
				throw new IntegrityException();
			
			AffectedDomains[a.Address] = a.Clone();
			AffectedDomains[a.Address].Affected  = true;;

			return AffectedDomains[a.Address];
		}

		public override void InitializeExecution()
		{
			Migrations	= Id == 0 ? new() : (Previous as RdsRound).Migrations;
		}

		public override void RestartExecution()
		{
			AffectedDomains.Clear();
		}

		public override void FinishExecution()
		{
			foreach(var a in AffectedDomains)
			{
				a.Value.Affected = false;

				if(a.Value.Resources != null)
					foreach(var r in a.Value.Resources.Where(i => i.Affected))
					{
						r.Affected = false;

						if(r.Outbounds != null)
							foreach(var l in r.Outbounds.Where(i => i.Affected))
								l.Affected = false;
					}
			}
		}

		public override void Elect(Vote[] votes, int gq)
		{
			ConsensusMigrations	= votes	.SelectMany(i => i.Migrations).Distinct()
										.Where(x => Migrations.Any(b => b.Id == x.OperationId) && votes.Count(b => b.Migrations.Contains(x)) >= gq)
										.Order().ToArray();
		}

		public override void CopyConfirmed()
		{
			Migrations = Migrations.ToList();
		}

		public override void RegisterForeign(Operation o)
		{
			if(o is DomainMigration m)
			{
				m.Generator = m.Transaction.Generator;
				Migrations.Add(m);
			}
		}

		public override void ConfirmForeign()
		{
			foreach(var i in ConsensusMigrations)
			{
				var e = Migrations.Find(j => j.Id == i.OperationId);

				if(i.Approved)
				{
					e.ConfirmedExecute(this);
					Migrations.Remove(e);
				} 
				else
					AffectAccount(Mcv.Accounts.Find(e.Generator, Id).Address).AvarageUptime -= 10;
			}

			Migrations.RemoveAll(i => Id > i.Id.Ri + Mcv.Zone.ExternalVerificationDurationLimit);
		}

		public override void WriteBaseState(BinaryWriter writer)
		{
			base.WriteBaseState(writer);

			writer.Write(Migrations, i => i.WriteBaseState(writer));
		}

		public override void ReadBaseState(BinaryReader reader)
		{
			base.ReadBaseState(reader);

			Migrations = reader.Read<DomainMigration>(m => m.ReadBaseState(reader)).ToList();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			base.WriteConfirmed(writer);

			writer.Write(ConsensusMigrations);
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			base.ReadConfirmed(reader);
			
			ConsensusMigrations	= reader.ReadArray<ForeignResult>();
		}
	}
}
