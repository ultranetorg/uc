using System.Diagnostics;

namespace Uccs.Fair
{
	public class AssortmentEntry : ITableEntry
	{
		public EntityId			Id { get; set; }
		public int				NextProductId { get; set; }
		public Product[]		Products { get; set; } = [];

		public bool				New;
		//public bool			Affected;
		FairMcv					Mcv;
		bool					ProductsCloned;

		public AssortmentEntry()
		{
		}

		public AssortmentEntry(FairMcv rdn)
		{
			Mcv = rdn;
		}

		public override string ToString()
		{
			return $"{Id}, Products={{{Products.Length}}}";
		}

		public AssortmentEntry Clone()
		{
			return new AssortmentEntry(Mcv){Id = Id,
										    Products = Products,
										    NextProductId = NextProductId};
		}

		public void WriteMain(BinaryWriter writer)
		{
			writer.Write7BitEncodedInt(NextProductId);
			writer.Write(Products, i =>	{
											writer.Write7BitEncodedInt(i.Id.P);
											i.WriteMain(writer);
										});
		}

		public void ReadMain(BinaryReader reader)
		{
			NextProductId	= reader.Read7BitEncodedInt();
			Products = reader.ReadArray(() =>	{ 
													var a = new Product();

													a.Id = new ProductId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
													a.ReadMain(reader);

													return a;
												});
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

		public void Cleanup(Round lastInCommit)
		{
		}

		public Product AffectProduct(ProductId id)
		{
			if(id != null)
			{
				if(!ProductsCloned && Products[id.P].Affected)
					throw new IntegrityException();

	 			var i = Products == null ? -1 : Array.FindIndex(Products, i => i.Id == id);

				if(!ProductsCloned)
				{
					Products = Products.ToArray();
					ProductsCloned = true;
				}

				if(!Products[i].Affected)
				{
					Products[i] = Products[i].Clone();
					Products[i].Affected = true;
				}
				
				return Products[i];
			} 
			else
			{
				var r = new Product {Affected = true,
									 New = true,
									 Id = new ProductId(Id.Ci, Id.Ei, NextProductId++)};

				Products = Products == null ? [r] : [..Products, r];
				ProductsCloned = true;

				return r;
			}
		}

		public void DeleteProduct(Product resource)
		{
			Products = Products.Where(i => i != resource).ToArray();
			ProductsCloned = true;
		}
	}
}
