namespace Uccs.Net;

public abstract class McvPpc<R> : Ppc<R> where R : Result
{
	public new McvPeering	Peering => base.Peering as McvPeering;
	public McvNode			Node => Peering.Node;
	public Mcv				Mcv => Node.Mcv;

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

		if(!Node.Mcv.NextVotingRound.Voters.Any(i => Node.Mcv.Settings.Generators.Any(j => j.Signer == i.Address))) 
			throw new NodeException(NodeError.NotMember);
	}

//	protected Generator RequireMemberFor(AccountAddress signer)
//	{
//		RequireGraph();
//
//		var m = Node.Mcv.NextVotingRound.VotersRound.Members.NearestBy(m => m.Address, signer);
//
//		if(!Node.Mcv.Settings.Generators.Any(i => i.Signer == m.Address)) 
//			throw new NodeException(NodeError.NotMember);
//
//		return m;
//	}
}

public class McvNode : Node
{
	public new McvNet		Net => base.Net as McvNet;
	public Mcv				Mcv;
	public McvPeering		Peering;
	public McvNodeSettings	Settings;

	public McvNode(string name, McvNet net, string profile, NexusSettings nexussettings, Flow flow) : base(name, net, profile, nexussettings,  flow)
	{
	}

	public override string ToString()
	{
		return Peering.ToString();
	}
}
