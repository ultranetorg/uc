namespace Uccs.Fair
{
	public class FairRound : Round
	{
		public new FairMcv							Mcv => base.Mcv as FairMcv;
		public Dictionary<EntityId, PublisherEntry>	AffectedPublishers = new();
		public Dictionary<ushort, int>				NextPublisherIds = new ();

		public FairRound(FairMcv rds) : base(rds)
		{
		}

		public override long AccountAllocationFee(Account account)
		{
			return FairOperation.SpacetimeFee(Uccs.Net.Mcv.EntityLength, Uccs.Net.Mcv.Forever);
		}

		public override IEnumerable<object> AffectedByTable(TableBase table)
		{
			if(table == Mcv.Accounts)
				return AffectedAccounts.Values;

			if(table == Mcv.Publishers)
				return AffectedPublishers.Values;

			throw new IntegrityException();
		}

		public PublisherEntry AffectPublisher(EntityId id)
		{
			ushort ci;

			if(id == null)
			{
				int pid;
				
				if(Mcv.Publishers.Clusters.Count() == 0)
				{	
					ci = 0;
					NextPublisherIds[ci] = 0;
				}
				else
				{	
					if(Mcv.Publishers.Clusters.Count() < TableBase.ClustersCountMax)
					{	
						var i = Enumerable.Range(0, TableBase.ClustersCountMax).First(i => Mcv.Publishers.Clusters.All(c => c.Id != i));
						ci = (ushort)i;
						NextPublisherIds[ci] = 0;
					}
					else	
					{	
						var c = Mcv.Publishers.Clusters.MinBy(i => i.BaseEntries.Count());
						ci = c.Id;
						NextPublisherIds[ci] = c.NextEntityId;
					}
				}
				
				pid = NextPublisherIds[ci]++;

				return AffectedPublishers[new EntityId(ci, pid)] = new PublisherEntry(Mcv){	Affected = true,
																							New = true,
																							Id = new EntityId(ci, pid)};
			}
			else
			{
				if(AffectedPublishers.TryGetValue(id, out PublisherEntry a))
					return a;
			
				var e = Mcv.Publishers.Find(id, Id - 1);

				if(e == null)
					throw new IntegrityException();

				AffectedPublishers[id] = e.Clone();
				AffectedPublishers[id].Affected  = true;

				return AffectedPublishers[id];
			}
		}

		public override void InitializeExecution()
		{
		}

		public override void RestartExecution()
		{
			AffectedPublishers.Clear();
			NextPublisherIds.Clear();
		}

		public override void FinishExecution()
		{
			foreach(var a in AffectedPublishers)
			{
				a.Value.Affected = false;

				if(a.Value.Products != null)
					foreach(var r in a.Value.Products.Where(i => i.Affected))
					{
						r.Affected = false;
					}
			}
		}

		public override void Elect(Vote[] votes, int gq)
		{
		}

		public override void CopyConfirmed()
		{
		}

		public override void RegisterForeign(Operation o)
		{
		}

		public override void ConfirmForeign()
		{
		}

		public override void WriteBaseState(BinaryWriter writer)
		{
			base.WriteBaseState(writer);

			writer.Write(Candidates, i => i.WriteCandidate(writer));  
			writer.Write(Members, i => i.WriteMember(writer));  
		}

		public override void ReadBaseState(BinaryReader reader)
		{
			base.ReadBaseState(reader);

			Candidates	= reader.Read<Generator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
			Members		= reader.Read<Generator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
		}
	}
}
