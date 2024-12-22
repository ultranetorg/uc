namespace Uccs.Net;

public abstract class McvPpc<R> : Ppc<R> where R : PeerResponse
{
	public new McvTcpPeering	Peering => base.Peering as McvTcpPeering;
	public new McvNode			Node => base.Node as McvNode;
	public Mcv					Mcv => Node.Mcv;

	protected void RequireBase()
	{
		if(Node.Mcv == null)
			throw new NodeException(NodeError.NotBase);

		if(Peering.Synchronization != Synchronization.Synchronized)
			throw new NodeException(NodeError.NotSynchronized);
	}

	protected void RequireMember()
	{
		RequireBase();

		if(!Node.Mcv.NextVoteRound.VotersRound.Members.Any(i => Node.Mcv.Settings.Generators.Contains(i.Address))) 
			throw new NodeException(NodeError.NotMember);
	}

	protected Generator RequireMemberFor(AccountAddress signer)
	{
		RequireBase();

		var m = Node.Mcv.NextVoteRound.VotersRound.Members.NearestBy(m => m.Address, signer);

		if(!Node.Mcv.Settings.Generators.Contains(m.Address)) 
			throw new NodeException(NodeError.NotMember);

		return m;
	}
}

public class McvNode : Node
{
	public McvNet			Net;
	public Vault			Vault; 
	public Mcv				Mcv;
	public McvTcpPeering	Peering;
	//public NodeSettings	Settings;

	public McvNode(string name, McvNet net, string profile, Flow flow, Vault vault) : base(name, net, profile, flow)
	{
		Net = net;
		Vault = vault;
	}

	public override string ToString()
	{
		return Peering.ToString();
	}
}
