namespace Uccs.Fair;

public class PublicationProductChange : VotableOperation
{
	public AutoId	Publication { get; set; }
	public AutoId	Product { get; set; }

	public override bool		IsValid(McvNet net) => true;
	public override string		Explanation => $"{Publication}, [{Product}]";

	public PublicationProductChange()
	{
	}

	public override void Read(BinaryReader reader)
	{
		Publication	= reader.Read<AutoId>();
		Product		= reader.Read<AutoId>();
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

		if(!RequireProduct(execution, Product, out var _, out var _))
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

			PayEnergyBySite(execution, s.Id);
		}

		var p = execution.Publications.Affect(Publication);
 		
		var r = execution.Products.Affect(p.Product);
		r.Publications = r.Publications.Remove(p.Id);

		p.Product = Product;

		r = execution.Products.Affect(Product);
		r.Publications = [..r.Publications, p.Id];

	}
}
