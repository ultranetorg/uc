namespace Uccs.Fair;

public class PublicationDeletion : FairOperation
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

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return;

		p = execution.Publications.Affect(Publication);
		p.Deleted = true;


 		var c = execution.Categories.Find(p.Category);
		var s = execution.Sites.Affect(c.Site);
		var a = execution.Authors.Find(execution.Products.Find(p.Product).Author);

		if(c.Publications.Contains(p.Id))
		{
			c = execution.Categories.Affect(c.Id);
			c.Publications = c.Publications.Remove(Publication);

			s.PublicationsCount--;
		}

		if(p.Flags.HasFlag(PublicationFlags.ApprovedByAuthor) && CanAccessAuthor(execution, a.Id))
		{ 
			a = execution.Authors.Affect(a.Id);

			Free(execution, a, a, execution.Net.EntityLength);
			PayEnergyByAuthor(execution, a.Id);
		}
		else if(CanAccessSite(execution, s.Id))
		{	
			Free(execution, s, s, execution.Net.EntityLength);
			PayEnergyBySite(execution, s.Id);
		}
		else
		{
			Error = Denied;
			return;
		}
		
		if(s.PendingPublications.Contains(p.Id))
		{
			s = execution.Sites.Affect(s.Id);
			s.PendingPublications = s.PendingPublications.Remove(p.Id);
		}

		var f = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
		
		if(f != null)
		{
			execution.PublicationTitles.Deindex(c.Site, execution.Products.Find(p.Product).Get(f).AsUtf8);
		}
	}
}