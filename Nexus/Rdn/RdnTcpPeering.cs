using System.Reflection;

namespace Uccs.Rdn;

public enum RdnPpcClass : byte
{
	None = 0, 
	Domain = McvPpcClass._Last + 1, 
	RdnMembers,
	QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease
}

public abstract class RdnPpc<R> : McvPpc<R> where R : Return
{
	public new RdnTcpPeering	Peering => base.Peering as RdnTcpPeering;
	public new RdnNode			Node => base.Node as RdnNode;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
}

public class RdnTcpPeering : McvTcpPeering
{
	public RdnTcpPeering(RdnNode node, PeeringSettings settings, long roles, VaultApiClient vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Constructor.Register<PeerRequest>	 (GetType().Assembly, typeof(RdnPpcClass), i => i.Remove(i.Length - "Ppc".Length));
		Constructor.Register<Return>	 (GetType().Assembly, typeof(RdnPpcClass), i => i.Remove(i.Length - "Ppr".Length));
		Constructor.Register<Urr>			 (GetType().Assembly, typeof(UrrScheme), i => i);

		Constructor.Register<Vote, RdnVote>(() => new RdnVote(Mcv));

		//var s = new MemoryStream();
		//var w = new BinaryWriter(s);
		//var r = new BinaryReader(s);
		//BinarySerializator.Serialize(w, new SharePeersPpc {Peers = [new Peer {IP = new([1,2,3,4])}]}, Constructor.TypeToCode); 
		//s.Position = 0;
		//var p = BinarySerializator.Deserialize<PeerRequest>(r, Constructor.Construct);


		Run();
	}

	public override bool ValidateIncoming(Operation o)
	{
		#if ETHEREUM
		if(o is Immission e && !Ethereum.IsEmissionValid(e))
			return false;
		#endif

		if(o is DomainMigration m && !(Node as RdnNode).IsDnsValid(m))
			return false;

		return true;
	}

	public override void OnRequestException(Peer peer, NodeException ex)
	{
		base.OnRequestException(peer, ex);

		if(ex.Error == NodeError.NotSeed)	peer.Roles  &= ~(long)RdnRole.Seed;
	}

}
