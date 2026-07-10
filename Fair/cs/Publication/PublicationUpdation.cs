using System.Text;

namespace Uccs.Fair;

public class PublicationUpdation : VotableOperation
{
	public AutoId				Publication { get; set; }
	public int					Version { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"Publication={Publication}, Version={Version}";

	public PublicationUpdation()
	{
	}

	public override void Read(Reader reader)
	{
		Publication	= reader.Read<AutoId>();
		Version		= reader.Read7BitEncodedInt();
	}

	public override void Write(Writer writer)
	{
		writer.Write(Publication);
		writer.Write7BitEncodedInt(Version);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationUpdation;

		return o.Publication == Publication;
	}
	
	public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		if(!p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{
			error = NotApproved;
			return false;
		}

		var r = execution.Products.Find(p.Product);

		if(!r.Versions.Any(i => i.Id == Version))
		{
			error = NotFound;
			return false;
		}

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var p = execution.Publications.Affect(Publication);
		var a = execution.Authors.Affect(execution.Products.Find(p.Product).Author);
		var r = execution.Products.Affect(p.Product);

		var v = r.Versions.First(i => i.Id == Version);
		var prev = r.Versions.FirstOrDefault(i => i.Id == p.ProductVersion);
				
		if(prev != null)	/// replace version
		{
			r.Versions = r.Versions.Replace(prev,	new ProductVersion
													{
														Id		= prev.Id, 
														Refs	= prev.Refs - 1,
														Fields	= prev.Fields
													});

			if(p.IsPublished)
				execution.ProductTitles.Deindex(p);
		}
	
		p.ProductVersion = Version;

		/// increase refs in product
	
		r.Versions = r.Versions.Replace(v,	new ProductVersion
											{
												Id		= v.Id, 
												Refs	= v.Refs + 1,
												Fields	= v.Fields
											});

		if(Version == r.Versions.Last().Id)
			Store.ChangedPublications = Store.ChangedPublications.Remove(p.Id);

		if(p.IsPublished)
			execution.ProductTitles.Index(p);

		execution.RewardForModeration(Store, a, out Error);
	}
}
