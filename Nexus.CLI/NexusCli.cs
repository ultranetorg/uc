using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Vault;

namespace Uccs.Nexus.CLI;

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
	public Nexus				Nexus;
	public NetBoot				Boot;
	public static HttpClient	ApiHttpClient;
	public NexusSettings		Settings;

	NexusApiClient				_Nexus;
	public NexusApiClient		NexusApi => _Nexus ??= new NexusApiClient(Settings.Api.LocalSystemAddress(Settings.Zone, Api.Nexus), null, ApiHttpClient);

	static NexusCli()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	public NexusCli()
	{
	}

	public static void Main(string[] args)
	{
		var cli = new NexusCli();
		cli.Boot = new NetBoot(ExeDirectory);
		cli.Settings = new NexusSettings(cli.Boot.Zone, cli.Boot.Profile);

		cli.Execute(cli.Boot);

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
