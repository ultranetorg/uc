namespace Uccs.Fair;

public class ProductCreation : FairOperation
{
	public AutoId				Author { get; set; }

	public override bool		IsValid(McvNet net) => true; // !Changes.HasFlag(ProductChanges.Description) || (Data.Length <= Product.DescriptionLengthMax);
	public override string		Explanation => $"{Author}";

	public ProductCreation()
	{
	}

	public ProductCreation(AutoId author)
	{
		Author	= author;
	}

	public override void Read(BinaryReader reader)
	{
		Author	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Author);
	}

	public override void Execute(FairExecution execution)
	{
		if(CanAccessAuthor(execution, Author, out var a, out Error) == false)
			return;

		a = execution.Authors.Affect(Author);
		var p = execution.Products.Create(a);

		p.Author = a.Id;
		a.Products = [..a.Products, p.Id];

		execution.Allocate(a, a, execution.Net.EntityLength);
		execution.PayCycleEnergy(a);
	}
}