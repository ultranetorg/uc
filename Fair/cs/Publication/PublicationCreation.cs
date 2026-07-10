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

	public override void Read(Reader reader)
	{
		Product = reader.Read<AutoId>();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Product);
	}
	
	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationCreation;

		return o.Store == Store && o.Product == Product;
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

		if(r.Publications.Any(i => execution.Publications.Find(i).Store == Store.Id))
		{	
			error = AlreadyExists;
			return false;
		}

		if(Store.UnpublishedPublications.Any(i => execution.Publications.Find(i).Product == Product))
		{	
			error = AlreadyExists;
			return false;
		}

		var t = r.Versions.LastOrDefault()?.Fields.FirstOrDefault(f => f.Name == Token.Title);

		if(t == null)
		{	
			error = NotReady;
			return false;
		}

		return true;
	}
	
	public override void Execute(FairExecution execution)
	{
		var r = execution.Products.Affect(Product);
		var p = execution.Publications.Create(Store);
		var a = execution.Authors.Affect(r.Author);
		
		var v = r.Versions.Last();

		p.Store				= Store.Id;
		p.Product			= r.Id;
		p.ProductVersion	= v.Id;
		p.AuthorRank		= a.VerifiedWebdomainRank;

		r.Versions		= [..r.Versions[..^1], new ProductVersion {Id = v.Id, Fields = v.Fields, Refs = v.Refs + 1}];
		r.Publications	= [..r.Publications, p.Id];

		Store.PublicationsCount++;
				
		if(!Store.Publishers.Any(i => i.Author == r.Author))
		{
			Store.Publishers = [..Store.Publishers,	new Publisher
													{
														Author = a.Id, 
														EnergyLimit = Publisher.Unlimit, 
														SpacetimeLimit = Publisher.Unlimit
													}];
			a.Stores = [..a.Stores, Store.Id];
		}

		Store.UnpublishedPublications = [..Store.UnpublishedPublications, p.Id];

		if(As == Role.Candidate || As == Role.Publisher)
		{ 
			p.Flags = PublicationFlags.RequestedByAuthor | PublicationFlags.ApprovedByAuthor;
			execution.Allocate(a, a, execution.Net.EntityLength);
			execution.PayOperationEnergy(a);
		}
		else
		{	
			execution.Allocate(Store, Store, execution.Net.EntityLength);
		}
	}
}