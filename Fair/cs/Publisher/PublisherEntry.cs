using System.Diagnostics;

namespace Uccs.Fair
{
	public class PublisherEntry : Publisher, ITableEntry
	{
		public bool				New;
		public bool				Affected;
		Mcv						Mcv;
		bool					ProductsCloned;
		
		public Product[]		Products { get; set; } = [];

		public PublisherEntry()
		{
		}

		public PublisherEntry(Mcv chain)
		{
			Mcv = chain;
		}

		public override string ToString()
		{
			return $"{Id}, {Owner}, {Expiration}";
		}

		public PublisherEntry Clone()
		{
			return new PublisherEntry(Mcv) {Id = Id,
											Owner = Owner,
											Expiration = Expiration,
											Products = Products,
											NextProductId = NextProductId,
											SpaceReserved = SpaceReserved,
											SpaceUsed = SpaceUsed,
											};
		}

		public void WriteMain(BinaryWriter writer)
		{
			var f = PublisherFlag.None;

			writer.Write((byte)f);
			writer.Write7BitEncodedInt(NextProductId);
			writer.Write7BitEncodedInt(SpaceReserved);
			writer.Write7BitEncodedInt(SpaceUsed);

			writer.Write(Owner);
			writer.Write(Expiration);

			writer.Write(Products, i =>{
											writer.Write7BitEncodedInt(i.Id.P);
											i.WriteMain(writer);
										});
		}

		public void Cleanup(Round lastInCommit)
		{
		}

		public void ReadMain(BinaryReader reader)
		{
			var f			= (PublisherFlag)reader.ReadByte();
			NextProductId	= reader.Read7BitEncodedInt();
			SpaceReserved	= (short)reader.Read7BitEncodedInt();
			SpaceUsed		= (short)reader.Read7BitEncodedInt();

			Owner		= reader.Read<EntityId>();
			Expiration	= reader.Read<Time>();

			Products = reader.Read(() =>	{ 
												var a = new Product();

												a.Id = new ProductId(Id.Ci, Id.Ei, reader.Read7BitEncodedInt());
												a.ReadMain(reader);

												return a;
											}).ToArray();
		}

		public void WriteMore(BinaryWriter w)
		{
		}

		public void ReadMore(BinaryReader r)
		{
		}

		public Product AffectProduct(ProductId id)
		{
			if(!Affected)
				Debugger.Break();

			if(id != null)
			{
				var i = Array.FindIndex(Products, i => i.Id.P == id.P);

				if(!ProductsCloned && Products[i].Affected)
					Debugger.Break();

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

				Products = Products == null ? [r] : Products.Append(r).ToArray();
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
