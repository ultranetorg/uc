using System.Text;

namespace Uccs.Fair
{
	public class AssortmentTable : Table<AssortmentEntry>
	{
		public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
		public new FairMcv				Mcv => base.Mcv as FairMcv;

		//public ushort					KeyToCluster(string domain) => Cluster.FromBytes(Encoding.UTF8.GetBytes(domain, 0, sizeof(ushort)));

		public AssortmentTable(FairMcv rds) : base(rds)
		{
		}
		
		protected override AssortmentEntry Create()
		{
			return new AssortmentEntry(Mcv);
		}

		public AssortmentEntry Find(EntityId id, int ridmax)
		{
			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
			//	throw new IntegrityException("maxrid works inside pool only");

			foreach(var r in Tail.Where(i => i.Id <= ridmax))
			{
				var a = r.AffectedAssortments.Values.FirstOrDefault(i => i.Id == id);
				
				if(a != null)
					return a;
			}

			return FindEntry(id);
		}
		
 		
  		public Product FindProduct(ProductId id, int ridmax)
  		{
 			//if(0 < ridmax && ridmax < Database.Tail.Last().Id - 1)
 			//	throw new IntegrityException("maxrid works inside pool only");
 
  			foreach(var r in Tail.Where(i => i.Id <= ridmax))
 			{
 				if(r.AffectedAssortments.TryGetValue(id.PublisherId, out var s))
                {
                    var x = s.Products.FirstOrDefault(i => i.Id.P == id.P);
 
 				    if(x != null)
 					    return x;
                }
 			}
  		
  			return FindEntry(new EntityId(id.C, id.D))?.Products.FirstOrDefault(i => i.Id.P == id.P);
  		}
	}
}
