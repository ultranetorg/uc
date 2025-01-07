using System.Reflection;

namespace Uccs.Rdn;

public enum RdnPpcClass : byte
{
	None = 0, 
	Domain = McvPpcClass._Last + 1, 
	RdnMembers,
	QueryResource, Resource, DeclareRelease, LocateRelease, FileInfo, DownloadRelease
}

public abstract class RdnPpc<R> : McvPpc<R> where R : PeerResponse
{
	public new RdnTcpPeering	Peering => base.Peering as RdnTcpPeering;
	public new RdnNode			Node => base.Node as RdnNode;
	public new RdnMcv			Mcv => base.Mcv as RdnMcv;
}

public class RdnTcpPeering : McvTcpPeering
{
	public RdnTcpPeering(RdnNode node, PeeringSettings settings, long roles, Vault vault, Flow flow, IClock clock) : base(node, settings, roles, vault, flow)
	{
		Register(typeof(RdnPpcClass), node);
 	 
		Contructors[typeof(Urr)] = [];

 		foreach(var i in Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Urr))))
 		{	
 			if(Enum.TryParse<UrrScheme>(i.Name, out var c))
 			{
 				Codes[i] = (byte)c;
			var x = i.GetConstructor([]);
 				Contructors[typeof(Urr)][(byte)c] = () => x.Invoke(null);
 			}
 		}

		Codes[typeof(ResourceException)] = (byte)ExceptionClass._Next;
		Contructors[typeof(NetException)][(byte)ExceptionClass._Next]  = () => typeof(ResourceException).GetConstructor([]).Invoke(null);

		Run();
	}

	public override object Constract(Type t, byte b)
	{
 			if(t == typeof(Vote))	
 				return new RdnVote(Mcv);

		return base.Constract(t, b);
	}

	public override bool ProcessIncomingOperation(Operation o)
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
