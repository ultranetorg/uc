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
		p.Site		= Site;
		p.Product	= pr.Id;
		p.Creator	= Signer.Id;
	
		s = execution.Sites.Affect(Site);
		s.PendingPublications = [..s.PendingPublications, p.Id];
		
		if(CanAccessAuthor(execution, a.Id))
		{ 
			a = execution.Authors.Affect(a.Id);

			p.Flags = PublicationFlags.ApprovedByAuthor;

			a.Energy	-= s.AuthorPublicationRequestFee;
			s.Energy	+= s.AuthorPublicationRequestFee;

			Allocate(execution, a, a, execution.Net.EntityLength);
			PayEnergyByAuthor(execution, a.Id);
		}
		else if(CanAccessSite(execution, Site))
		{	
			Allocate(execution, s, s, execution.Net.EntityLength);
			PayEnergyBySite(execution, s.Id);
		}
		else
		{
			Error = Denied;
			return;
		}

		var r = execution.Products.Affect(pr.Id);
		r.Publications = [..r.Publications, p.Id];

	}
}