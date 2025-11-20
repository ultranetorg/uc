using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus;

public class NodeInstance
{
	public string		ApiLocalAddress { get; set; }
	public string		Net;

	public override string ToString()
	{
		return Net;
	}
}

public class NexusCli : Cli
{
	public Nexus Nexus;

	public NexusCli()
	{
	}

	public static void Main(string[] args)
	{
		var b = new NetBoot(ExeDirectory);
		var ns = new NexusSettings(b.Zone, b.Profile) {Name = Guid.NewGuid().ToString()};
		var vs = new VaultSettings(b.Profile, b.Zone);
		
		var cli = new NexusCli();

		cli.Nexus = new Nexus(b, ns, vs, new RealClock(), new Flow(nameof(Nexus), new Log()));

		cli.Nexus.RunRdn(null);

		cli.Execute(b);

		cli.Nexus.Stop();
	}
//
//	public override NexusCommand Create(IEnumerable<Xon> commnad, Flow flow)
//	{
//		var t = commnad.First().Name;
//		var args = commnad.Skip(1).ToList();
//		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());
//
//		return ct.GetConstructor([typeof(NexusCli), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as NexusCommand;
//	}
}	
