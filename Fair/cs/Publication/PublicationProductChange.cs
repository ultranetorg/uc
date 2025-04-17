namespace Uccs.Fair;

public class PublicationProductChange : VotableOperation
{
	public EntityId	Publication { get; set; }
	public EntityId	Product { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}, [{Product}]";

	public PublicationProductChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<EntityId>();
		Product		= reader.Read<EntityId>();
	}

	public override void Write(BinaryWriter writer)
	{
		writer.Write(Publication);
		writer.Write(Product);
	}

	public override bool Overlaps(VotableOperation other)
	{
		return (other as PublicationProductChange).Publication == Publication;
	}
	
	public override bool ValidProposal(FairExecution execution)
	{
		if(!RequirePublication(execution, Publication, out var p))
			return false;

		return p.Product != Product;
	}

	public override void Execute(FairExecution execution, bool dispute)
	{
		if(!ValidProposal(execution))
			return;

		if(!dispute)
	 	{
			if(!RequirePublicationModeratorAccess(execution, Publication, Signer, out var _, out var s))
				return;

	 		if(s.ChangePolicies[FairOperationClass.PublicationProductChange] != ChangePolicy.AnyModerator)
	 		{
		 		Error = Denied;
		 		return;
	 		}
		}

		var p = execution.AffectPublication(Publication);
 		
		var r = execution.AffectProduct(p.Product);
		r.Publications = r.Publications.Remove(p.Id);

		p.Product = Product;

		r = execution.AffectProduct(Product);
		r.Publications = [..r.Publications, p.Id];
	}
}
