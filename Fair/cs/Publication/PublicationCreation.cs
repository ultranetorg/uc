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
					
		var p = execution.CreatePublication(execution.FindSite(c.Site));
		
		a = execution.AffectAuthor(a.Id);
		var s = execution.AffectSite(c.Site);

		if(!a.Sites.Contains(c.Site))
			a.Sites = [..a.Sites, c.Site];

		if(!s.Authors.Contains(a.Id))
			s.Authors = [..s.Authors, a.Id];

		if(CanAccessAuthor(execution, a.Id))
		{ 
			p.Status = PublicationStatus.Pending;
			p.Flags = PublicationFlags.CreatedByAuthor;
						
			Allocate(execution, a, a, execution.Net.EntityLength);

			EnergySpenders.Add(a);
			SpacetimeSpenders.Add(a);
		}
		else if(CanAccessSite(execution, c.Site))
		{	
			p.Status = PublicationStatus.Pending;
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

		var r = execution.AffectProduct(Product);
		r.Publications = [..r.Publications, p.Id];

		c = execution.AffectCategory(c.Id);
		c.Publications = [..c.Publications, p.Id];
	}
}