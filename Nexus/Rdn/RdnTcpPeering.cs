using System.Reflection;

namespace Uccs.Rdn;

public enum RdnPpcClass : uint
{
	None = 0, 
	Domain = McvPpcClass._Last + 1, 
	RdnMembers,
	ResourceByAddress, ResourceById, QueryResource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease
}

public abstract class RdnPpc<R> : McvPpc<R> where R : Result
{
	public new RdnTcpPeering	Peering => base.Peering as RdnTcpPeering;
	public new RdnNode			Node => base.Node as RdnNode;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
}

public class RdnTcpPeering : McvPeering
{
	public RdnTcpPeering(RdnNode node, PeeringSettings settings, long roles, VaultApiClient vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Constructor.Register<PeerRequest>(GetType().Assembly, typeof(RdnPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Result>	 (GetType().Assembly, typeof(RdnPpcClass), i => i.Remove(i.Length - "Ppr".Length));
		Constructor.Register<Urr>		 (GetType().Assembly, typeof(UrrScheme), i => i);

		Constructor.Register<Vote, RdnVote>(() => new RdnVote(Mcv));

		Run();
	}

	public override bool ValidateIncoming(Operation o)
	{
		/// TODO: remake as async
		/// if(o is DomainMigration m && !Node.IsWebdomainOwner($"{m.Name}.{m.Tld}", ))
		/// 	return false;

		return true;
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);

		if(ex.Error == NodeError.NotSeed)	peer.Roles  &= ~(long)RdnRole.Seed;
	}

}
