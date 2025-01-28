using System.Reflection;

namespace Uccs.Smp;

public enum SmpPpcClass : byte
{
	None = 0, 
	SmpMembers = McvPpcClass._Last + 1, 

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
}

public abstract class SmpPpc<R> : McvPpc<R> where R : PeerResponse
{
	public new SmpTcpPeering	Peering => base.Peering as SmpTcpPeering;
	public new FairNode			Node => base.Node as FairNode;
	public new SmpMcv			Mcv => base.Mcv as SmpMcv;
}

public class SmpTcpPeering : McvTcpPeering
{
	public SmpTcpPeering(FairNode node, PeeringSettings settings, long roles, Vault vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Register(typeof(SmpPpcClass), node);

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
