using System.Reflection;

namespace Uccs.Fair;

public enum FairPpcClass : byte
{
	None = 0, 
	FairAccount = McvPpcClass._Last + 1, 
	FairMembers,

	AccountAuthors,
	AccountSites,

	Author,
	Product,
	Site,
	SiteCategories,
	Category,
	CategoryPublications,
	CategoryCategories,
	Publication,
	Review,
	Pow
}

public abstract class FairPpc<R> : McvPpc<R> where R : PeerResponse
{
	public new FairTcpPeering	Peering => base.Peering as FairTcpPeering;
	public new FairNode			Node => base.Node as FairNode;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
}

public class FairTcpPeering : McvTcpPeering
{
	public FairTcpPeering(FairNode node, PeeringSettings settings, long roles, UosApiClient vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Register(typeof(FairPpcClass), node);

		Run();
	}

	public override object Constract(Type t, byte b)
	{
 		if(t == typeof(FairAccount))	
 			return new FairAccount(Mcv);

		return base.Constract(t, b);
	}
	public override bool ValidateIncoming(Operation o)
	{
		return o is not VotableOperation;
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);
	}
}
