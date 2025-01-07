using System.Reflection;

namespace Uccs.Fair;

public enum FairPpcClass : byte
{
	None = 0, 
	Author = McvPpcClass._Last + 1, 
	Product,
	Site,
	Cost,
	FairMembers,
	AccountAuthors,
	AccountSites,
	Page,
	SitePages
}

public abstract class FairPpc<R> : McvPpc<R> where R : PeerResponse
{
	public new FairTcpPeering	Peering => base.Peering as FairTcpPeering;
	public new FairNode			Node => base.Node as FairNode;
	public new FairMcv			Mcv => base.Mcv as FairMcv;
}

public class FairTcpPeering : McvTcpPeering
{
	public FairTcpPeering(FairNode node, PeeringSettings settings, long roles, Vault vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Register(typeof(FairPpcClass), node);

		Run();
	}

	public override bool ProcessIncomingOperation(Operation o)
	{
		return true;
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);
	}
}
