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
							PublicationChange.ApproveChange	=> reader.Read<ProductFieldVersionId>(),
							PublicationChange.RejectChange	=> reader.Read<ProductFieldVersionId>(),
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
			case PublicationChange.ApproveChange : writer.Write(Value as ProductFieldVersionId); break;
			case PublicationChange.RejectChange	 : writer.Write(Value as ProductFieldVersionId); break;
			default : throw new IntegrityException();
		}
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);

		switch(Change)
		{
			case PublicationChange.Status:
			{ 
 				var s = (PublicationStatus)Value;
 			
				var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
 
 				if(p.Status == PublicationStatus.Pending && s == PublicationStatus.Approved)
 				{
					a.Energy -= a.ModerationReward;
					Signer.Energy += a.ModerationReward;

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
				var v = Value as ProductFieldVersionId;
				var r = round.AffectProduct(p.Product);

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Id == v.Id);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];

				var rf = r.Fields.First(i => i.Name == v.Name);
				
				if(rf != null)
				{
					var pf = p.Fields.FirstOrDefault(i => i.Name == v.Name);
	
					if(pf == null)
						p.Fields = [..p.Fields, v];
					else
						p.Fields = [..p.Fields.Where(i => i.Name != v.Name), v];
		
					if(pf != null)
					{
						var x = rf.Versions.First(i => i.Id == pf.Id);
	
						rf = new ProductField {Name = rf.Name, 
											   Versions = [..rf.Versions.Where(i => i.Id != x.Id), new ProductFieldVersion {Id = x.Id, Value = x.Value, Refs = x.Refs - 1}]};
		
						r.Fields = [..r.Fields.Where(i => i.Name != rf.Name), rf];
					}
	
					var y = rf.Versions.First(i => i.Id == v.Id);
	
					rf = new ProductField {Name = rf.Name, 
										  Versions = [..rf.Versions.Where(i => i.Id != y.Id), new ProductFieldVersion {Id = y.Id, Value = y.Value, Refs = y.Refs + 1}]};
	
					r.Fields = [..r.Fields.Where(i => i.Name != v.Name), rf];
				} 
				else
				{
					p.Fields = [..p.Fields.Where(i => i.Name != v.Name)];
				}

				var a = round.AffectAuthor(r.Author);

				a.Energy -= a.ModerationReward;
				Signer.Energy += a.ModerationReward;

				break;
			}

			case PublicationChange.RejectChange:
			{	
				var v = Value as ProductFieldVersionId;

				var c = p.Changes.FirstOrDefault(i => i.Name == v.Name && i.Id == v.Id);

				if(c == null)
				{
					Error = NotFound;
					return;
				}
				
				p.Changes = [..p.Changes.Where(i => i != c)];

				var a = round.AffectAuthor(mcv.Products.Find(p.Product, round.Id).Author);

				a.Energy -= a.ModerationReward;
				Signer.Energy += a.ModerationReward;

				break;
			}

			case PublicationChange.Product:
				p.Product = EntityId;
				break;
		}
	}
}