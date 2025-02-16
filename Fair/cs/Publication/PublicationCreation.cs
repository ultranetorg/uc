namespace Uccs.Fair;

public class PublicationCreation : FairOperation
{
	public EntityId					Product { get; set; }
	public EntityId					Category { get; set; }
	//public ProductFieldVersionId[]	Fields { get; set; }

	public override bool		IsValid(Mcv mcv) => Product != null && Category != null;
	public override string		Description => $"Product={Product} Category={Category}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Product = reader.Read<EntityId>();
		Category= reader.Read<EntityId>();
		//Fields	= reader.ReadArray<ProductFieldVersionId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Category);
		//writer.Write(Fields);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequireProduct(round, Product, out var a, out var pr))
			return;

		if(!RequireCategory(round, Category, out var c))
			return;
					
		var p = round.CreatePublication(mcv.Sites.Find(c.Site, round.Id));

		if(Signer.Id == a.Owner)
		{ 
			p.Status = PublicationStatus.Pending;
			p.Flags = PublicationFlags.CreatedByAuthor;
			
			a = round.AffectAuthor(a.Id);
			
			Allocate(round, a, a, Mcv.EntityLength);
		}
		else if(mcv.Sites.Find(c.Site, mcv.LastConfirmedRound.Id)?.Moderators.Contains(Signer.Id) ?? false)
		{	
			p.Status = PublicationStatus.Pending;
			p.Flags = PublicationFlags.CreatedBySite;

			var s = round.AffectSite(c.Site);

			Allocate(round, s, s, Mcv.EntityLength);
		}
		else
		{
			Error = Denied;
			return;
		}

		p.Product	= Product;
		p.Category	= Category;
		p.Creator	= Signer.Id;

		var r = round.AffectProduct(Product);
		r.Publications = [..r.Publications, p.Id];

		c = round.AffectCategory(c.Id);
		c.Publications = [..c.Publications, p.Id];
	}
}