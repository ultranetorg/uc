namespace Uccs.Fair;

public enum PublicationChange : byte
{
	None,
	Status,
	ApproveChange,
	RejectChange,
	Product,
}

public class PublicationUpdation : UpdateOperation
{
	public EntityId				Publication { get; set; }
	public PublicationChange	Change { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Change}]";

	public PublicationUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Change		= reader.ReadEnum<PublicationChange>();
		
		Value = Change switch
					   {
							PublicationChange.Status		=> reader.ReadEnum<PublicationStatus>(),
							PublicationChange.Product		=> reader.Read<EntityId>(),
							PublicationChange.ApproveChange	=> reader.Read<ProductFieldVersionReference>(),
							PublicationChange.RejectChange	=> reader.Read<ProductFieldVersionReference>(),
							_ => throw new IntegrityException()
					   };
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.WriteEnum(Change);

		switch(Change)
		{
			case PublicationChange.Status		 : writer.WriteEnum((PublicationStatus)Value); break;
			case PublicationChange.Product		 : writer.Write(EntityId); break;
			case PublicationChange.ApproveChange : writer.Write(Value as ProductFieldVersionReference); break;
			case PublicationChange.RejectChange	 : writer.Write(Value as ProductFieldVersionReference); break;
			default : throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);

		void pay(AuthorEntry a)
		{
			var s = round.AffectSite(round.FindCategory(p.Category).Site);

			a.Energy	  -= a.ModerationReward;
			Signer.Energy += a.ModerationReward;
			
			EnergyFeePayer = s;
			EnergySpenders.Add(s);
			EnergySpenders.Add(a);
		}

		switch(Change)
		{
			case PublicationChange.Status:
			{ 
 				var s = (PublicationStatus)Value;
 			
				var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
 
 				if(p.Status == PublicationStatus.Pending && s == PublicationStatus.Approved)
 				{
					pay(a);

					p.Status = PublicationStatus.Approved;
 				}
				else
				{
					Error = NotAvailable;
					return;
				}
 
				p.Status = (PublicationStatus)Value;
				break;
			}

			case PublicationChange.ApproveChange:
			{
				var v = Value as ProductFieldVersionReference;
				var r = round.AffectProduct(p.Product);

				var f = r.Fields.First(i => i.Name == v.Name);

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Version == v.Version);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];
								
				if(f != null)
				{
					var prev = p.Fields.FirstOrDefault(i => i.Name == v.Name);
	
					if(prev == null)	/// new field
						p.Fields = [..p.Fields, v];
					else			/// replace version
						p.Fields = [..p.Fields.Where(i => i.Name != v.Name), v];
		
					if(prev != null) /// if previously used then decrease refs in product
					{
						var x = f.Versions.First(i => i.Version == prev.Version);
	
						f = new ProductField {Name = f.Name, 
											  Versions = [..f.Versions.Where(i => i.Version != x.Version), new ProductFieldVersion(x.Version, x.Value, x.Refs - 1)]};
		
						r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
					}
	
					/// increase refs in product
					
					var y = f.Versions.First(i => i.Version == v.Version);
	
					f = new ProductField {Name = f.Name, 
										  Versions = [..f.Versions.Where(i => i.Version != y.Version), new ProductFieldVersion(y.Version, y.Value, y.Refs + 1)]};
	
					r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
				} 
				else /// a field is deleted from product
				{
					p.Fields = [..p.Fields.Where(i => i.Name != v.Name)];
				}

				var a = round.AffectAuthor(r.Author);
				
				pay(a);
				
				break;
			}

			case PublicationChange.RejectChange:
			{	
				var v = Value as ProductFieldVersionReference;

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Version == v.Version);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];

				var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
				
				pay(a);
				
				break;
			}

			case PublicationChange.Product:
				p.Product = EntityId;
				break;
		}
	}
}