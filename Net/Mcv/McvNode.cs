
using DnsClient;

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

		if(!Node.Mcv.NextVotingRound.Senders.Any(i => Node.Mcv.Settings.Generators.Any(j => j.Id == i.User))) 
			throw new NodeException(NodeError.NotMember);
	}
}

public abstract class McvNode : Node
{
	public new McvNet				Net => base.Net as McvNet;
	public Mcv						Mcv;
	public McvPeering				Peering;
	public McvIccpLcpConnection		Iccp;
	public McvNodeSettings			Settings;
	public Thread					GuiThread;
	public Action					ShowGui;
	LookupClient					Dns = new LookupClient(new LookupClientOptions {Timeout = TimeSpan.FromSeconds(5)});

	public abstract byte[]			Do(string query);

	public McvNode(McvNet net, string profile, NexusSettings nexussettings, Flow flow) : base(net, profile, nexussettings,  flow)
	{
	}

	public override string ToString()
	{
		return Peering.ToString();
	}

	public bool IsWebdomainOwner(string domain, AutoId user)
	{
		if(NodeGlobals.ForceApproveOutwards)
			return true;

		try
		{
			var result = Dns.QueryAsync(domain, QueryType.TXT, QueryClass.IN, Flow.Cancellation);

			return result.Result.Answers.TxtRecords().Any(r => r.DomainName == domain + '.' && AutoId.TryParse(r.Text.First(), out var id) && id == user);
		}
		catch(Exception)
		{
		}

		return false;
	}

}
