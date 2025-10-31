using System.Net;
using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;

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

public class Nexus : Cli
{
	public delegate void			Delegate(Nexus d);

	public NexusSettings			Settings;
	public List<NodeInstance>		Nodes = [];
	internal NexusApiServer			ApiServer;
	public static HttpClient		ApiHttpClient;
	public RdnNode					RdnNode;
	//RdnApiClient					_Rdn;
	//public RdnApiClient				RdnApi => _Rdn ??= new RdnApiClient(Settings.Api.LocalAddress(Rdn.Rdn.ByZone(Settings.Zone)), null, ApiHttpClient);
	public PackageHub				PackageHub;

	public Delegate					Stopped;

	public NodeInstance				Find(string net) => Nodes.Find(i => i.Net == net);

	static Nexus()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	public static void Main(string[] args)
	{
		var b = new NetBoot(ExeDirectory);
		var ns = new NexusSettings(b) {Name = Guid.NewGuid().ToString()};
		var rs = new RdnNodeSettings(b.Profile);
		
		var u = new Nexus(b, ns, rs, new RealClock(), new Flow(nameof(Nexus), new Log()));

		u.Execute(b);

		u.Stop();
	}

	public Nexus(NetBoot boot, NexusSettings settings, RdnNodeSettings rdnsettings, IClock clock, Flow flow)
	{
		Settings = settings ?? new NexusSettings(boot);
		Settings.Packages = Settings.Packages ?? Path.Join(boot.Profile, "Packages");
		Flow = flow;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, $"{GetType().Name}.{FailureExt}"))
				File.Delete(i);
		
		RdnNode = new RdnNode(Settings.Name, Settings.Zone, boot.Profile, rdnsettings, clock, flow);
		PackageHub = new PackageHub(RdnNode, Settings.Packages);

		Nodes = [new NodeInstance {Net = Rdn.Rdn.Root}];

		if(Settings.Api != null)
		{
			RunApi();
		}
	}

	public override string ToString()
	{
		return string.Join(" - ", new string[]{	Settings.Name,
												ApiServer != null ? "A" : null,
												///Rdn?.ToString()
												}.Where(i => i != null));
	}

	public void Stop()
	{
		Flow.Abort();

		Stopped?.Invoke(this);

		ApiServer?.Stop();

		foreach(var i in Nodes.ToArray())
		{	
			//i.Node.Stop();
			Nodes.Remove(i);
		}
	}
	
	public void RunApi()
	{
		if(!HttpListener.IsSupported)
		{
			Environment.ExitCode = -1;
			throw new RequirementException("Windows XP SP2, Windows Server 2003 or higher is required to use the application.");
		}

		if(ApiServer != null)
			throw new NodeException(NodeError.AlreadyRunning);

		ApiServer = new NexusApiServer(this, Flow);
	
		//ApiStarted?.Invoke(this);
	}

	public McvApiClient GetMcvNodeApi(string net)
	{
		var ni = Find(net);

		return new McvApiClient(ni.ApiLocalAddress, null, ApiHttpClient);
	}

	public override NexusCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;
		var args = commnad.Skip(1).ToList();
		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct.GetConstructor([typeof(Nexus), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as NexusCommand;
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.ProfileKey,			Settings.Profile);
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
