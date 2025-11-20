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
	public FairTcpPeering(FairNode node, PeeringSettings settings, long roles, VaultApiClient vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Constructor.Register<PeerRequest>	 (typeof(FairPpcClass), i => i.Remove(i.Length - "Ppc".Length), i => i.Node = node);
		Constructor.Register<FuncPeerRequest>(typeof(FairPpcClass), i => i.Remove(i.Length - "Ppc".Length), i => i.Node = node);
		Constructor.Register<ProcPeerRequest>(typeof(FairPpcClass), i => i.Remove(i.Length - "Ppc".Length), i => i.Node = node);
		Constructor.Register<PeerResponse>	 (typeof(FairPpcClass), i => i.Remove(i.Length - "Ppr".Length));

		Constructor.Register(() => new FairAccount(Mcv));

		Run();
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
