namespace Uccs.Net;

public abstract class McvPpc<R> : Ppc<R> where R : PeerResponse
{
	public new McvTcpPeering	Peering => base.Peering as McvTcpPeering;
	public new McvNode			Node => base.Node as McvNode;
	public Mcv					Mcv => Node.Mcv;

	protected void RequireGraph()
	{
		if(Node.Mcv == null)
			throw new NodeException(NodeError.NotGraph);

		if(Peering.Synchronization != Synchronization.Synchronized)
			throw new NodeException(NodeError.NotSynchronized);
	}

	protected void RequireMember()
	{
		RequireGraph();

		if(!Node.Mcv.NextVoteRound.VotersRound.Members.Any(i => Node.Mcv.Settings.Generators.Contains(i.Address))) 
			throw new NodeException(NodeError.NotMember);
	}

	protected Generator RequireMemberFor(AccountAddress signer)
	{
		RequireGraph();

		var m = Node.Mcv.NextVoteRound.VotersRound.Members.NearestBy(m => m.Address, signer);

		if(!Node.Mcv.Settings.Generators.Contains(m.Address)) 
			throw new NodeException(NodeError.NotMember);

		return m;
	}
}

public class McvNode : Node
{
	public new McvNet		Net => base.Net as McvNet;
	public UosApiClient		UosApi; 
	public Mcv				Mcv;
	public McvTcpPeering	Peering;
	public McvNodeSettings	Settings;

	public McvNode(string name, McvNet net, string profile, Flow flow, UosApiClient vault) : base(name, net, profile, flow)
	{
		UosApi = vault;
	}

	public override string ToString()
	{
		return Peering.ToString();
	}
}
