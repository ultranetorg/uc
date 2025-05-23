namespace Uccs.Fair;

public class PublicationCreation : FairOperation
{
	public AutoId					Product { get; set; }
	public AutoId					Category { get; set; }
	//public ProductFieldVersionId[]	Fields { get; set; }

	public override bool		IsValid(McvNet net) => Product != null && Category != null;
	public override string		Explanation => $"Product={Product} Category={Category}";

	public override void Read(BinaryReader reader)
	{
		Product = reader.Read<AutoId>();
		Category= reader.Read<AutoId>();
		//Fields	= reader.ReadArray<ProductFieldVersionId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Category);
		//writer.Write(Fields);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireProduct(execution, Product, out var a, out var pr))
			return;

		if(!RequireCategory(execution, Category, out var c))
			return;

		if(pr.Publications.Any(i => execution.Categories.Find(execution.Publications.Find(i).Category).Site == c.Site))
		{
			Error = AlreadyExists;
			return;
		}
					
		var p = execution.Publications.Create(execution.Sites.Find(c.Site));
		
		a = execution.Authors.Affect(a.Id);
		var s = execution.Sites.Affect(c.Site);

		if(!a.Sites.Contains(c.Site))
			a.Sites = [..a.Sites, c.Site];

		if(!s.Authors.Contains(a.Id))
			s.Authors = [..s.Authors, a.Id];

		if(CanAccessAuthor(execution, a.Id))
		{ 
			p.Flags = PublicationFlags.CreatedByAuthor;
						
			Allocate(execution, a, a, execution.Net.EntityLength);

			EnergySpenders.Add(a);
			SpacetimeSpenders.Add(a);
		}
		else if(CanAccessSite(execution, c.Site))
		{	
			p.Flags = PublicationFlags.CreatedBySite;

			Allocate(execution, s, s, execution.Net.EntityLength);

			EnergySpenders.Add(s);
			SpacetimeSpenders.Add(s);
		}
		else
		{
			Error = Denied;
			return;
		}

		p.Product	= Product;
		p.Category	= Category;
		p.Creator	= Signer.Id;

		var r = execution.Products.Affect(Product);
		r.Publications = [..r.Publications, p.Id];

		s.PendingPublications = [..s.PendingPublications, p.Id];
	}
}