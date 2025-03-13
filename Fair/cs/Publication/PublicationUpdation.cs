namespace Uccs.Fair;

public abstract class PublicationUpdation : FairOperation
{
	protected void Pay(FairRound round, Publication publication, AuthorEntry author)
	{
		var s = round.AffectSite(round.FindCategory(publication.Category).Site);

		author.Energy -= author.ModerationReward;
		Signer.Energy += author.ModerationReward;
			
		EnergyFeePayer = s;
		EnergySpenders.Add(s);
		EnergySpenders.Add(author);
	}
}

public class PublicationStatusUpdation : PublicationUpdation
{
	public EntityId				Publication { get; set; }
	public PublicationStatus	Status { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string		Description => $"{GetType().Name}, [{Status}]";

	public PublicationStatusUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Status		= reader.ReadEnum<PublicationStatus>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.WriteEnum(Status);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;
 
 		if(p.Status == PublicationStatus.Pending && Status == PublicationStatus.Approved) /// When Author asked to publish
 		{
			p =	round.AffectPublication(Publication);
			var a = round.AffectAuthor(round.FindProduct(p.Product).Author);

			Pay(round, p, a);

			p.Status = PublicationStatus.Approved;
 		}
		else
		{
			Error = NotAvailable;
			return;
		}
	}
}

public class PublicationProductUpdation : PublicationUpdation
{
	public EntityId	Publication { get; set; }
	public EntityId	Product { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{GetType().Name}, [{Product}]";

	public PublicationProductUpdation()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Product);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);
 			
		var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
 
		p.Product = Product;
	}
}

public class PublicationChangeModeration : PublicationUpdation
{
	public EntityId						Publication { get; set; }
	public ProductFieldVersionReference	Change { get; set; }
	public bool							Resolution { get; set; }

	public override bool				IsValid(Mcv mcv) => Publication != null; // !Changes.HasFlag(CardChanges.Description) || (Data.Length <= Card.DescriptionLengthMax);
	public override string				Description => $"{GetType().Name}, {Change}, {Resolution}";

	public PublicationChangeModeration()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Change		= reader.Read<ProductFieldVersionReference>();
		Resolution	= reader.ReadBoolean();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Change);
		writer.Write(Resolution);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p))
			return;

		p = round.AffectPublication(Publication);
 			
		var a = round.AffectAuthor(round.FindProduct(p.Product).Author);
 
		if(Resolution == true)
		{
			var r = round.AffectProduct(p.Product);

			var f = r.Fields.First(i => i.Name == Change.Name);

			var c = p.Changes.FirstOrDefault(i => i.Name == Change.Name && i.Version == Change.Version);

			if(c == null)
			{
				Error = NotFound;
				return;
			}
				
			p.Changes = [..p.Changes.Where(i => i != c)];
								
			if(f != null)
			{
				var prev = p.Fields.FirstOrDefault(i => i.Name == Change.Name);
	
				if(prev == null)	/// new field
					p.Fields = [..p.Fields, Change];
				else			/// replace version
					p.Fields = [..p.Fields.Where(i => i.Name != Change.Name), Change];
		
				if(prev != null) /// if previously used then decrease refs in product
				{
					var x = f.Versions.First(i => i.Version == prev.Version);
	
					f = new ProductField {Name = f.Name, 
											Versions = [..f.Versions.Where(i => i.Version != x.Version), new ProductFieldVersion(x.Version, x.Value, x.Refs - 1)]};
		
					r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
				}
	
				/// increase refs in product
					
				var y = f.Versions.First(i => i.Version == Change.Version);
	
				f = new ProductField {Name = f.Name, 
										Versions = [..f.Versions.Where(i => i.Version != y.Version), new ProductFieldVersion(y.Version, y.Value, y.Refs + 1)]};
	
				r.Fields = [..r.Fields.Where(i => i.Name != f.Name), f];
			} 
			else /// a field is deleted from product
			{
				p.Fields = [..p.Fields.Where(i => i.Name != Change.Name)];
			}

			Pay(round, p, a);
		}
		else
		{	
			var c = p.Changes.FirstOrDefault(i => i.Name == Change.Name && i.Version == Change.Version);

			if(c == null)
			{
				Error = NotFound;
				return;
			}
				
			p.Changes = [..p.Changes.Where(i => i != c)];

			Pay(round, p, a);
		}
	}
}
