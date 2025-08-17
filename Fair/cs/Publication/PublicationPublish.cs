namespace Uccs.Fair;

public class PublicationPublish : VotableOperation
{
	public AutoId				Publication { get; set; }
	public AutoId				Category { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}, [{Category}]";

	public PublicationPublish()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Category	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Category);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationPublish).Publication == Publication;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		if(!CategoryExists(execution, Category, out var c, out error))
			return false;

		if(c.Site != Site.Id)
		{	
			error = DoesNotBelogToSite;
			return false;
		}

		if(p.Site != Site.Id)
		{	
			error = DoesNotBelogToSite;
			return false;
		}

		return p.Category != Category;
	}

	public override void Execute(FairExecution execution)
	{
		var p = execution.Publications.Affect(Publication);
		var r = execution.Products.Find(p.Product);

		if(p.Category != Category && p.Category != null)
		{
			var x = execution.Categories.Affect(p.Category);
			x.Publications = x.Publications.Remove(p.Id);
		}

		p.Category = Category;
	
		var c = execution.Categories.Affect(Category);
		c.Publications = [..c.Publications, p.Id];

		Site.UnpublishedPublications = [..Site.UnpublishedPublications, p.Id];

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor))
		{ 
			var a = execution.Authors.Affect(r.Author);

			RewardForModeration(execution, a, Site);
		}

		var tr = r.Versions.Last().Fields.FirstOrDefault(f => f.Name == Token.Title);
			
		if(tr != null)
			execution.PublicationTitles.Index(Site.Id, p.Id, tr.AsUtf8);
	}
}
