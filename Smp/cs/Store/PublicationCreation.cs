namespace Uccs.Smp;

public class PublicationCreation : SmpOperation
{
	public EntityId				Product { get; set; }
	public EntityId				Category { get; set; }

	public override bool		IsValid(Mcv mcv) => Product != null && Category != null;
	public override string		Description => $"{GetType().Name} Product={Product} Category={Category}";

	public override void ReadConfirmed(BinaryReader reader)
	{
		Product = reader.Read<EntityId>();
		Category = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Category);
	}

	public override void Execute(SmpMcv mcv, SmpRound round)
	{
		if(!RequireProduct(round, Product, out var a, out var pr))
			return;

		if(!RequireCategory(round, Category, out var c))
			return;
					
		var p = round.CreatePublication(mcv.Sites.Find(c.Site, round.Id));

		p.Product	= Product;
		p.Category	= Category;
		p.Creator	= Signer.Id;
		
		c = round.AffectCategory(c.Id);
		
		c.Publications ??= [];
		c.Publications = [..c.Publications, p.Id];
	}
}