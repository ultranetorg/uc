namespace Uccs.Fair;

public class PublicationDeletion : FairOperation
{
	public EntityId				Publication { get; set; }

	public override bool		IsValid(Mcv mcv) => Publication != null;
	public override string		Description => $"{Id}";

	public PublicationDeletion()
	{
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication = reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublication(round, Publication, out var p))
			return;

		p = round.AffectPublication(Publication);
		p.Deleted = true;

 		var c = round.AffectCategory(p.Category);
 		c.Publications = c.Publications.Remove(Publication);

		var a = round.FindAuthor(round.FindProduct(p.Product).Author);

		if(((p.Flags & PublicationFlags.CreatedByAuthor) == PublicationFlags.CreatedByAuthor) && a.Owners.Contains(Signer.Id))
		{ 
			a = round.AffectAuthor(a.Id);

			Free(round, a, a, mcv.Net.EntityLength);

			EnergySpenders.Add(a);
		}
		else if(((p.Flags & PublicationFlags.CreatedBySite) == PublicationFlags.CreatedBySite) && (round.FindSite(c.Site)?.Moderators.Contains(Signer.Id) ?? false))
		{	
			var s = round.AffectSite(c.Site);

			Free(round, s, s, mcv.Net.EntityLength);

			EnergySpenders.Add(s);
		}
		else
		{
			Error = Denied;
			return;
		}
	}
}