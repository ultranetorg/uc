namespace Uccs.Rdn
{
	public class RdnRound : Round
	{
		public List<DomainMigration>			Migrations;
		public new RdnMcv						Mcv => base.Mcv as RdnMcv;
		public Dictionary<string, DomainEntry>	AffectedDomains = new();
		public Dictionary<byte[], int>			NextDomainIds;
		public ForeignResult[]					ConsensusMigrations = {};

		public RdnRound(RdnMcv rds) : base(rds)
		{
		}

		public override long AccountAllocationFee(Account account)
		{
			return RdnOperation.SpacetimeFee(Uccs.Net.Mcv.EntityLength, Uccs.Net.Mcv.Forever);
		}

		public override IEnumerable<object> AffectedByTable(TableBase table)
		{
			if(table == Mcv.Accounts)
				return AffectedAccounts.Values;

			if(table == Mcv.Domains)
				return AffectedDomains.Values;

			throw new IntegrityException();
		}

		public DomainEntry AffectDomain(string domain)
		{
			if(AffectedDomains.TryGetValue(domain, out DomainEntry a))
				return a;
			
			var e = Mcv.Domains.Find(domain, Id - 1);

			if(e != null)
			{
				AffectedDomains[domain] = e.Clone();
				AffectedDomains[domain].Affected  = true;;
				return AffectedDomains[domain];
			}
			else
			{
				var ci = Mcv.Domains.KeyToCluster(domain).ToArray();
				var c = Mcv.Domains.Clusters.FirstOrDefault(i => i.Id.SequenceEqual(ci));

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
			
			a = Mcv.Domains.Find(id, Id - 1);

			if(a == null)
				throw new IntegrityException();
			
			AffectedDomains[a.Address] = a.Clone();
			AffectedDomains[a.Address].Affected  = true;;

			return AffectedDomains[a.Address];
		}

		public override void InitializeExecution()
		{
			Migrations	= Id == 0 ? new() : (Previous as RdnRound).Migrations;
		}

		public override void RestartExecution()
		{
			AffectedDomains.Clear();
			NextDomainIds	= new (Bytes.EqualityComparer);
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
			var vs = votes.Cast<RdnVote>();

			ConsensusMigrations	= vs.SelectMany(i => i.Migrations).Distinct()
									 .Where(x => Migrations.Any(b => b.Id == x.OperationId) && vs.Count(b => b.Migrations.Contains(x)) >= gq)
									 .Order().ToArray();

			#if IMMISSION
			ConsensusEmissions	= rvs.SelectMany(i => i.Emissions).Distinct()
									 .Where(x => Emissions.Any(e => e.Id == x.OperationId) && rvs.Count(b => b.Emissions.Contains(x)) >= gq)
									 .Order().ToArray();
			#endif
		}

		public override void CopyConfirmed()
		{
			Migrations = Migrations.ToList();
		}

		public override void RegisterForeign(Operation o)
		{
			if(o is DomainMigration m)
			{
				m.Generator = m.Transaction.Member;
				Migrations.Add(m);
			}
		}

		public override void ConfirmForeign()
		{
			foreach(var i in ConsensusNtnStates)
			{
				var b = Mcv.NtnBlocks.Find(j => j.State.Hash.SequenceEqual(i));

				if(b == null)
					throw new ConfirmationException(this, []);

				var d = AffectDomain(b.Net);
				d.NtnSelfHash	= b.State.Hash;
				d.NtnChildNet		= b.State;

				Mcv.NtnBlocks.Remove(b);
			}

			#if IMMISSION
			foreach(var i in ConsensusEmissions)
			{
				var e = Emissions.Find(j => j.Id == i.OperationId);

				if(i.Approved)
				{
					e.ConfirmedExecute(this);
					Emissions.Remove(e);
				} 
				else
					AffectAccount(Mcv.Accounts.Find(e.Generator, Id).Address).AvarageUptime -= 10;
			}

			Emissions.RemoveAll(i => Id > i.Id.Ri + Mcv.Net.ExternalVerificationDurationLimit);
			#endif

			foreach(var i in ConsensusMigrations)
			{
				var e = Migrations.Find(j => j.Id == i.OperationId);

				if(i.Approved)
				{
					e.ConfirmedExecute(this);
					Migrations.Remove(e);
				} 
				else
					AffectAccount(Mcv.Accounts.Find(e.Generator, Id).Address).AverageUptime -= 10;
			}

			Migrations.RemoveAll(i => Id > i.Id.Ri + Mcv.Net.ExternalVerificationRoundDurationLimit);
		}

		public override void WriteBaseState(BinaryWriter writer)
		{
			base.WriteBaseState(writer);

			writer.Write(Candidates, i => i.WriteCandidate(writer));  
			writer.Write(Members, i => i.WriteMember(writer));  
			writer.Write(Migrations, i => i.WriteBaseState(writer));
		}

		public override void ReadBaseState(BinaryReader reader)
		{
			base.ReadBaseState(reader);

			Candidates	= reader.Read<RdnGenerator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
			Members		= reader.Read<RdnGenerator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
			Migrations	= reader.Read<DomainMigration>(m => m.ReadBaseState(reader)).ToList();
		}

		public override void WriteConfirmed(BinaryWriter writer)
		{
			base.WriteConfirmed(writer);

			#if IMMISSION
			writer.Write(ConsensusEmissions);
			#endif
			writer.Write(ConsensusMigrations);
		}

		public override void ReadConfirmed(BinaryReader reader)
		{
			base.ReadConfirmed(reader);
			
			#if IMMISSION
			ConsensusEmissions	= reader.ReadArray<ForeignResult>();
			#endif
			ConsensusMigrations	= reader.ReadArray<ForeignResult>();
		}
	}
}
