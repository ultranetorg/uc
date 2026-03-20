namespace Uccs.Fair;

public class PublicationUnpublish : VotableOperation
{
	public AutoId				Publication { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}]";

	public PublicationUnpublish()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationUnpublish).Publication == Publication;
	}
	
	 public override bool ValidateProposal(FairExecution execution, out string error)
	{
		if(!PublicationExists(execution, Publication, out var p, out error))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
//		var p = execution.Publications.Affect(Publication);
//		var r = execution.Products.Find(p.Product);
//		var x = execution.Categories.Affect(p.Category);
//
//		x.Publications = x.Publications.Remove(p.Id);
//		Site.UnpublishedPublications = [..Site.UnpublishedPublications, p.Id];
//
//		var tr = r.Versions.Last().Fields.FirstOrDefault(f => f.Name == Token.Title);
//			
//		if(tr != null)
//			execution.PublicationTitles.Deindex(Site.Id, tr.AsUtf8);

		var p = execution.Publications.Affect(Publication);
		var	c = execution.Categories.Affect(p.Category);
		var r = execution.Products.Affect(p.Product);

		c.Publications				 = c.Publications.Remove(Publication);
		r.Publications				 = r.Publications.Remove(r.Id);
		Site.UnpublishedPublications = [..Site.UnpublishedPublications, p.Id];
		
		var f = r.Versions.First(i => i.Id == p.ProductVersion).Fields.FirstOrDefault(f => f.Name == Token.Title);
		
		if(f != null)
		{
			execution.PublicationTitles.Deindex(c.Site, f.AsUtf8);
		}
	}
}
