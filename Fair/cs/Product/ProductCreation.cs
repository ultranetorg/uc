namespace Uccs.Fair;

public class ProductCreation : FairOperation
{
	public ProductType			Type { get; set; }
	public AutoId				Author { get; set; }
	public string				Title { get; set; }

	public override bool		IsValid(McvNet net) => !string.IsNullOrWhiteSpace(Title) && Title.Length is > 0 and <= Product.TitleLengthMaximum; // !Changes.HasFlag(ProductChanges.Description) || (Data.Length <= Product.DescriptionLengthMax);
	public override string		Explanation => $"{Author}";

	public ProductCreation()
	{
	}

	public override void Read(Reader reader)
	{
		Title	= reader.ReadUtf8();
		Type	= reader.Read<ProductType>();
		Author	= reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.WriteUtf8(Title);
		writer.Write(Type);
		writer.Write(Author);
	}

	public override void Execute(FairExecution execution)
	{
		if(CanAccessAuthor(execution, Author, out var a, out Error) == false)
			return;

		a = execution.Authors.Affect(Author);
		var p = execution.Products.Create(a);

		p.Title = Title;
		p.Type = Type;
		p.Author = a.Id;

		a.Products = [..a.Products, p.Id];

		execution.Allocate(a, a, execution.Net.EntityLength);
		execution.PayOperationEnergy(a);
	}
}