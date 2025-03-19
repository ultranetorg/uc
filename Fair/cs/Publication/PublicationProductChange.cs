namespace Uccs.Fair;

public class PublicationProductChange : VotableOperation
{
	public EntityId	Publication { get; set; }
	public EntityId	Product { get; set; }

	public override bool		IsValid(Mcv mcv) => true;
	public override string		Description => $"{Publication}, [{Product}]";

	public PublicationProductChange()
	{
	}
	
	public override bool ValidProposal(FairMcv mcv, FairRound round, Site site)
	{
		if(!RequirePublication(round, Publication, out var p))
			return false;

		return p.Product != Product;
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationProductChange).Publication == Publication;
	}

	public override void ReadConfirmed(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
	}

	public override void WriteConfirmed(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Product);
	}

	public override void Execute(FairMcv mcv, FairRound round)
	{
		if(!RequirePublicationAccess(round, Publication, Signer, out var p, out var s))
			return;

 		if(s.ChangePolicies[FairOperationClass.PublicationProductChange] != ChangePolicy.AnyModerator)
 		{
 			Error = Denied;
 			return;
 		}

		Execute(mcv, round, s);
	}
	
	public override void Execute(FairMcv mcv, FairRound round, SiteEntry site)
	{
		var p = round.AffectPublication(Publication);
 		
		var r = round.AffectProduct(p.Product);
		r.Publications = r.Publications.Remove(p.Id);

		p.Product = Product;

		r = round.AffectProduct(Product);
		r.Publications = [..r.Publications, p.Id];
	}
}
