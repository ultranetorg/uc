using System.Text;

namespace Uccs.Fair;

public class PublicationDeletion : VotableOperation
{
	public AutoId				Publication { get; set; }

	public override bool		IsValid(McvNet net) => Publication != null;
	public override string		Explanation => $"{Publication}";

	public PublicationDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication = reader.Read<AutoId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override bool Overlaps(VotableOperation other)
	{
		var o = other as PublicationDeletion;

		return o.Publication == Publication;
	}
	
	public override bool ValidateProposal(FairExecution execution)
	{
		if(!PublicationExists(execution, Publication, out _, out _))
			return false;

		return true;
	}

	public override void Execute(FairExecution execution)
	{
		var p = execution.Publications.Affect(Publication);
		p.Deleted = true;

 		var c = execution.Categories.Find(p.Category);
		var s = execution.Sites.Affect(c.Site);
		var r = execution.Products.Affect(p.Product);
		//var a = execution.Authors.Find(.Author);

		r.Publications = r.Publications.Remove(r.Id);

		if(c.Publications.Contains(p.Id))
		{
			c = execution.Categories.Affect(c.Id);
			c.Publications = c.Publications.Remove(Publication);

			s.PublicationsCount--;
		}
		
		if(s.UnpublishedPublications.Contains(p.Id))
		{
			s.UnpublishedPublications = s.UnpublishedPublications.Remove(p.Id);
		}

		foreach(var i in p.Reviews)
		{
			execution.Reviews.Delete(s, i);
		}
		
		var f = p.Fields.FirstOrDefault(i => i.Field == ProductFieldName.Title);
		
		if(f != null)
		{
			execution.PublicationTitles.Deindex(c.Site, execution.Products.Find(p.Product).Get(f).AsUtf8);
		}

		execution.Free(s, s, execution.Net.EntityLength);
	}
}