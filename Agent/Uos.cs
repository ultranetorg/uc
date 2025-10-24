using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using Uccs.Net;

namespace Uccs.Uos;

public class NodeInstance
{
	public string		ApiLocalAddress { get; set; }
	//public int			ApiPort { get; set; }
	//public McvNode		Node;
	public string		Net;

	public override string ToString()
	{
		return Net;
	}
}

public class Uos : Cli
{
	public delegate void			Delegate(Uos d);
 	public delegate void			McvDelegate(Mcv d);

	public UosSettings				Settings;
	public List<NodeInstance>		Nodes = [];
	internal UosApiServer			ApiServer;
	public IClock					Clock;
	public Delegate					Stopped;
	public static HttpClient		ApiHttpClient;

	public NodeInstance				Find(string net) => Nodes.Find(i => i.Net == net);

	RdnApiClient					_Rdn;
	public RdnApiClient				RdnApi => _Rdn ??= new RdnApiClient(ApiHttpClient, Settings.Api.LocalAddress(Rdn.Rdn.ByZone(Settings.Zone)));

	static Uos()
	{
  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	static void Main(string[] args)
	{
		var boot = new NetBoot(ExeDirectory);
		var s = new UosSettings(boot.Profile, Guid.NewGuid().ToString(), boot.Zone);
		var u = new Uos(s, new Flow(nameof(Uos), new Log()), new RealClock());

		u.Execute(boot);

		u.Stop();
	}

	public Uos(UosSettings settings, Flow flow, IClock clock)
	{
		Settings = settings;
		Flow = flow;
		Clock = clock;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + FailureExt))
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

		ApiServer = new UosApiServer(this, Flow);
	
		//ApiStarted?.Invoke(this);
	}

	public McvApiClient GetMcvNodeApi(string net)
	{
		var ni = Find(net);

		return new McvApiClient(ApiHttpClient, ni.ApiLocalAddress, null);
	}

	public override UosCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;
		var args = commnad.Skip(1).ToList();
		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct.GetConstructor([typeof(Uos), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as UosCommand;
	}

	public VersionManifest GetCurrentManifest(ApvAddress address)
	{
		var h = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, address), "." + VersionManifest.Extension);
		
		return File.Exists(h) ? VersionManifest.FromXon(new Xon(File.ReadAllText(h))) : null;
	}

	public void Start(Unea address, Flow flow)
	{
		if(address.Net == Net.Net.Root || address.Net is null)
		{
			var ura = Ura.Parse(address.ToString());

			var d = RdnApi.FindLocalResource(ura, flow)?.Last
					?? 
					RdnApi.Request<ResourceResponse>(new PpcApc {Request = new ResourceRequest {Identifier = new (ura)}}, flow)?.Resource?.Data;
	
			if(d == null)
				throw new UosException("Incorrect resource type");
	
			//Ura apr = null;
			Ura aprv = null;
	
			if(d.Type.Content == ContentType.Rdn_ProductManifest)
			{
				var lrr = RdnApi.Download(ura, flow);
	
				var m = ProductManifest.FromXon(new Xon(new StreamReader(new MemoryStream(RdnApi.Request<byte[]>(new LocalReleaseReadApc {Address = lrr.Address, Path=""}, flow)), Encoding.UTF8).ReadToEnd()));
	
				aprv = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Latest;
			}
			else if(d.Type.Content == ContentType.Rdn_VersionManifest)
			{
				aprv = ura;
			}
			else
				throw new UosException("Incorrect resource type");
	
			RdnApi.DeployPackage(aprv, Settings.Packages, flow);
	
	 		var vmpath = Directory.EnumerateFiles(PackageHub.AddressToDeployment(Settings.Packages, aprv), "*." + VersionManifest.Extension).First();
	 
	 		var vm = VersionManifest.Load(vmpath);
	 
			var exe = vm.MatchExecution(Platform.Current);
	
			SetupApplicationEnvironemnt(aprv);
	
	 		var ps = new Process();
	 		ps.StartInfo.UseShellExecute = true;
	 		ps.StartInfo.FileName = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, aprv), exe.Path);
	 		ps.StartInfo.Arguments = exe.Arguments;
	
	 		ps.Start();
		}
		else
		{
			///if(Find(address.Net) == null)
			///{
			///	ConnectNetwork(address.Net);
			///}
			///
			///GetMcvNodeApi(address.Net).Send(new StartApc {Entity = address.Entity}, flow);
		}
	}

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.UosApiIPEnvKey,		Settings.Api.LocalIP.ToString());
		//Environment.SetEnvironmentVariable(Application.ApiKeyEnvKey,		Settings.Api.AccessKey);
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());
		Environment.SetEnvironmentVariable(Application.PackagesPathKey,		Settings.Packages);

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
