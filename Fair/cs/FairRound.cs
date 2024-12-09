﻿namespace Uccs.Fair
{
	public class FairRound : Round
	{
		public new FairMcv								Mcv => base.Mcv as FairMcv;
		public Dictionary<EntityId, PublisherEntry>		AffectedPublishers = new();
		public Dictionary<ushort, int>					NextPublisherIds = new ();
		public Dictionary<EntityId, AssortmentEntry>	AffectedAssortments = new();
		public Dictionary<ushort, int>					NextAssortmentIds = new ();

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
				if(Mcv.Publishers.Clusters.Count() == 0)
				{	
					ci = 0;
					NextPublisherIds[ci] = 0;
				}
				else
				{	
					if(Mcv.Publishers.Clusters.Count() < TableBase.ClustersCountMax)
					{	
						var i = Mcv.Publishers.Clusters.Count();
						ci = (ushort)i;
						NextPublisherIds[ci] = 0;
					}
					else	
					{	
						var c = Mcv.Publishers.Clusters.MinBy(i => i.BaseEntries.Count());
						ci = c.Id;
						NextPublisherIds[ci] = c.NextEntryId;
					}
				}
				
				var pid = NextPublisherIds[ci]++;

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

		public AssortmentEntry AffectAssortment(EntityId id)
		{
			if(AffectedAssortments.TryGetValue(id, out var a))
				return a;
			
			var e = Mcv.Assortments.Find(id, Id - 1);

			if(e != null)
			{
				AffectedAssortments[id] = e.Clone();
				//AffectedAssortments[domain].Affected  = true;;
				return AffectedAssortments[id];
			}
			else
			{
				var c = Mcv.Assortments.Clusters.FirstOrDefault(i => i.Id == id.Ci);

				int i;
				
				if(c == null)
					NextAssortmentIds[id.Ci] = 0;
				else
					NextAssortmentIds[id.Ci] = c.NextEntryId;
				
				i = NextAssortmentIds[id.Ci]++;

				return AffectedAssortments[id] = new AssortmentEntry(Mcv){	//Affected = true,
																			New = true,
																			Id = new EntityId(id.Ci, i)};
			}
		}

		public override void InitializeExecution()
		{
		}

		public override void RestartExecution()
		{
			AffectedPublishers.Clear();
			NextPublisherIds.Clear();
			AffectedAssortments.Clear();
			NextAssortmentIds.Clear();
		}

		public override void FinishExecution()
		{
			foreach(var a in AffectedAssortments)
			{
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
