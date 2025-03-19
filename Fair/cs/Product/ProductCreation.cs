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

	public override void Execute(FairExecution execution)
	{
		if(RequireAuthorAccess(execution, Author, out var a) == false)
			return;

		a = execution.AffectAuthor(Author);
		var p = execution.CreateProduct(a);

		p.Author = a.Id;
		a.Products = [..a.Products, p.Id];

		Allocate(execution, Signer, a, execution.Net.EntityLength);
	}
}