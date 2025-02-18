namespace Uccs.Fair;

public class ProductCreation : FairOperation
{
	public EntityId				Author { get; set; }

	public override bool		IsValid(Mcv mcv) => true; // !Changes.HasFlag(ProductChanges.Description) || (Data.Length <= Product.DescriptionLengthMax);
	public override string		Description => $"{Author}";

	public ProductCreation()
	{
	}

	public ProductCreation(EntityId author)
	{
		Author	= author;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Author	= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Author);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(RequireAuthorAccess(round, Author, out var a) == false)
			return;

		a = round.AffectAuthor(Author);
		var p = round.CreateProduct(a);

		p.Author = a.Id;
		a.Products = [..a.Products, p.Id];

		Allocate(round, Signer, a, mcv.Net.EntityLength);
	}
}