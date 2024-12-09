﻿using System.Text;

namespace Uccs.Fair
{
	public class PublisherTable : Table<PublisherEntry>
	{
		public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();

		//public override bool			Equal(object a, object b) => a.Equals(b);
		//public override Span<byte>		KeyToCluster(object domain) => null;

		public PublisherTable(FairMcv rds) : base(rds)
		{
		}
		
		protected override PublisherEntry Create()
		{
			return new PublisherEntry(Mcv);
		}

		public PublisherEntry Find(EntityId id, int ridmax)
		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Tail.Where(i => i.Id <= ridmax))
			{
				var a = r.AffectedPublishers.Values.FirstOrDefault(i => i.Id == id);
				
				if(a != null)
					return a;
			}

			return FindEntry(id);
		}
	}
}
