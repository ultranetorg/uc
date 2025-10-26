using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos;

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

	public HostSettings				Settings;
	public List<NodeInstance>		Nodes = [];
	internal NexusApiServer			ApiServer;
	public static HttpClient		ApiHttpClient;
	RdnApiClient					_Rdn;
	public RdnApiClient				RdnApi => _Rdn ??= new RdnApiClient(ApiHttpClient, Settings.Api.LocalAddress(Rdn.Rdn.ByZone(Settings.Zone)));

	public Delegate					Stopped;

	public NodeInstance				Find(string net) => Nodes.Find(i => i.Net == net);

	static Nexus()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	static void Main(string[] args)
	{
		var boot = new NetBoot(ExeDirectory);
		var s = new HostSettings(boot.Profile, Guid.NewGuid().ToString(), boot.Zone);
		var u = new Nexus(s, new Flow(nameof(Nexus), new Log()));

		u.Execute(boot);

		u.Stop();
	}

	public Nexus(HostSettings settings, Flow flow)
	{
		Settings = settings;
		Flow = flow;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, $"{GetType().Name}.{FailureExt}"))
				File.Delete(i);
			
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

		return new McvApiClient(ApiHttpClient, ni.ApiLocalAddress, null);
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
