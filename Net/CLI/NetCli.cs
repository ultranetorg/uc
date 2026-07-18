using System.Reflection;

namespace Uccs.Net;

public abstract class NetCli : Cli
{
	public NetBoot					Boot;
	public NexusSettings			NexusSettings;
	public abstract JsonApiClient	Api {get; }

	public NetCli()
	{
	}

	public NetCli(NexusSettings nexussettings)
	{
		NexusSettings = nexussettings;
	}

	public override Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return CreateFromAssembly(Assembly.GetExecutingAssembly(), commnad, flow);
	}
}
