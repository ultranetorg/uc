namespace Uccs.Fair;

public class PublicationCreation : VotableOperation
{
	public AutoId				Product { get; set; }

	public override bool		IsValid(McvNet net) => Product != null;
	public override string		Explanation => $"Product={Product}";

	public PublicationCreation()
	{
	}

	public PublicationCreation(AutoId product)
	{
		Product = product;
	}

	public override void Read(BinaryReader reader)
	{
		Product = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Product);
	}
	
	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationCreation;

		return o.Site == Site && o.Product == Product;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(ProductExists(execution, Product, out var a, out var r, out error) == false)
			return false;

		if(r.Versions == null)
		{
			error = NotReady;
			return false;
		}

		if(r.Publications.Any(i => execution.Publications.Find(i).Site == Site.Id))
		{	
			error = AlreadyExists;
			return false;
		}

		if(Site.UnpublishedPublications.Any(i => execution.Publications.Find(i).Product == Product))
		{	
			error = AlreadyExists;
			return false;
		}

		return true;
	}
	
	public override void Execute(FairExecution execution)
	{
		var r = execution.Products.Affect(Product);
		var p = execution.Publications.Create(Site);
		
		if(As == Role.Candidate)
		{ 
			p.Flags = PublicationFlags.ApprovedByAuthor;
		}

		var v = r.Versions.Last();

		p.Site				= Site.Id;
		p.Product			= r.Id;
		p.ProductVersion	= v.Id;

		r.Versions		= [..r.Versions[..^1], new ProductVersion {Id = v.Id, Fields = v.Fields, Refs = v.Refs + 1}];
		r.Publications	= [..r.Publications, p.Id];

		Site.PublicationsCount++;

		if(!Site.Publishers.Any(i => i.Author == r.Author))
		{
			var a = execution.Authors.Affect(r.Author);

			Site.Publishers = [..Site.Publishers, new Publisher {Author = a.Id}];
			a.Sites = [..a.Sites, Site.Id];
		}

		Site.UnpublishedPublications = [..Site.UnpublishedPublications, p.Id];

		execution.Allocate(Site, Site, execution.Net.EntityLength);
	}
}