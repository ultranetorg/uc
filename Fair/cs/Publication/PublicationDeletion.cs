namespace Uccs.Fair;

public class PublicationDeletion : FairOperation
{
	public EntityId				Publication { get; set; }

	public override bool		IsValid(McvNet net) => Publication != null;
	public override string		Explanation => $"{Id}";

	public PublicationDeletion()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication = reader.Read<EntityId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return;

		p = execution.AffectPublication(Publication);
		p.Deleted = true;

 		var c = execution.AffectCategory(p.Category);
 		c.Publications = c.Publications.Remove(Publication);

		var a = execution.FindAuthor(execution.FindProduct(p.Product).Author);

		if(((p.Flags & PublicationFlags.CreatedByAuthor) == PublicationFlags.CreatedByAuthor) && a.Owners.Contains(Signer.Id))
		{ 
			a = execution.AffectAuthor(a.Id);

			Free(execution, a, a, execution.Net.EntityLength);

			EnergySpenders.Add(a);
		}
		else if(((p.Flags & PublicationFlags.CreatedBySite) == PublicationFlags.CreatedBySite) && (execution.FindSite(c.Site)?.Moderators.Contains(Signer.Id) ?? false))
		{	
			var s = execution.AffectSite(c.Site);

			Free(execution, s, s, execution.Net.EntityLength);

			EnergySpenders.Add(s);
		}
		else
		{
			Error = Denied;
			return;
		}

		var f = p.Fields.FirstOrDefault(i => i.Name == ProductField.Title);
		
		if(f != null)
		{
			execution.UnregisterWord(execution.FindProduct(p.Product).GetString(f), EntityTextField.PublicationTitle, p.Id);
		}
	}
}