namespace Uccs.Fair;

public class PublicationCreation : FairOperation
{
	public AutoId					Product { get; set; }
	public AutoId					Site { get; set; }

	public override bool		IsValid(McvNet net) => Product != null && Site != null;
	public override string		Explanation => $"Product={Product} Site={Site}";

	public override void Read(BinaryReader reader)
	{
		Product = reader.Read<AutoId>();
		Site	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
		writer.Write(Site);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequireProduct(execution, Product, out var a, out var pr))
			return;

		if(!RequireSite(execution, Site, out var s))
			return;

		if(pr.Publications.Any(i => execution.Publications.Find(i).Site == Site))
		{
			Error = AlreadyExists;
			return;
		}
					
		var p = execution.Publications.Create(s);
		p.Site = Site;
	
		s = execution.Sites.Affect(Site);
		
		if(CanAccessAuthor(execution, a.Id))
		{ 
			a = execution.Authors.Affect(a.Id);

			p.Flags = PublicationFlags.CreatedByAuthor;
						
			Allocate(execution, a, a, execution.Net.EntityLength);

			EnergySpenders.Add(a);
			SpacetimeSpenders.Add(a);
		}
		else if(CanAccessSite(execution, Site))
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
		p.Creator	= Signer.Id;

		var r = execution.Products.Affect(Product);
		r.Publications = [..r.Publications, p.Id];

		s.PendingPublications = [..s.PendingPublications, p.Id];
	}
}