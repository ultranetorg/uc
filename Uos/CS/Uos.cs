﻿using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using Uccs.Net;

namespace Uccs.Uos;

public class NodeInstance
{
	public ApiSettings	ApiSettings { get; set; }
	public int			ApiPort { get; set; }
	public McvNode		Node;
	public string		Net;

	public override string ToString()
	{
		return Node.ToString();
	}
}

public class Uos : Cli
{
	public delegate void		Delegate(Uos d);
 	public delegate void		McvDelegate(Mcv d);

	public Func<string, AccountAddress, AuthenticationChioce>	AuthenticationRequested;
	public Action<AccountAddress>								UnlockRequested;
	public Func<string, AccountAddress, bool>					AuthorizationRequested;

	public static bool			ConsoleAvailable { get; protected set; }
	public IPasswordAsker		PasswordAsker = new ConsolePasswordAsker();
	public Flow					Flow = new Flow("uos", new Log()); 
	public static string		ExeDirectory;
	public UosSettings			Settings;
	public List<NodeInstance>	Nodes = [];
	internal UosApiServer		ApiServer;
	public IClock				Clock;
	public Delegate				Stopped;
	public Vault				Vault;
	static Boot					Boot;
	public static				ConsoleLogView	LogView = new ConsoleLogView(false, false);
	public static HttpClient	ApiHttpClient;

	public NodeInstance			Find(string net) => Nodes.Find(i => i.Net == net);
	public N					Find<N>() where N : class => Nodes.Find(i => i.Node is N)?.Node as N;

	RdnApiClient				_Rdn;
	public RdnApiClient			RdnApi => _Rdn ??= new RdnApiClient(ApiHttpClient, Nodes.Find(i => i.Net == Settings.Rdn.Address).ApiSettings.ListenAddress, Nodes.Find(i => i.Net == Settings.Rdn.Address).ApiSettings.AccessKey);
	//public McvApiClient			GetMcvApi(string net) => new McvApiClient(ApiHttpClient, Nodes.Find(i => i.Net == net).ApiSettings.ListenAddress, Nodes.Find(i => i.Net == net).ApiSettings.AccessKey);

	public NodeDelegate			NodeStarted;

	static Uos()
	{
		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
	
		try
		{
			var p = Console.KeyAvailable;
			ConsoleAvailable = true;
		}
		catch(Exception)
		{
			ConsoleAvailable = false;
		}

  	  	var h = new HttpClientHandler();
		h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
		ApiHttpClient = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};
	}

	static void Main(string[] args)
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

		Boot = new Boot(ExeDirectory);
		var s = new UosSettings(Boot.Profile, Guid.NewGuid().ToString(), Rdn.Rdn.ByZone(Boot.Zone));
		
		var u = new Uos(s, new Flow(nameof(Uos), new Log()), new RealClock());

		try
		{
			u.Execute(Boot.Commnand.Nodes, u.Flow);
		}
		catch(NetException ex) when (!Debugger.IsAttached)
		{
			u.Flow?.Log.ReportError(ex.Message);
		}

		u.Stop();
	}

	public Uos(UosSettings settings, Flow flow, IClock clock)
	{
		Settings = settings;
		Flow = flow;
		Clock = clock;

		new FileLog(Flow.Log, Flow.Name, Settings.Profile);

		Settings.Packages ??= Path.Join(Settings.Profile, nameof(Settings.Packages));
		Directory.CreateDirectory(Settings.Packages);

		Vault = new Vault(Settings.Profile, settings.EncryptVault);

		//Environment.SetEnvironmentVariable(BootProductsPath,productspath);
		//Environment.SetEnvironmentVariable(BootSunAddress,	sunaddress);
		//Environment.SetEnvironmentVariable(BootSunApiKey,	sunapikey);
		//Environment.SetEnvironmentVariable(BootNet,		net.Name);

		//ReportPreambule();
		//ReportNetwork();
		if(Directory.Exists(Settings.Profile))
			foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Node.FailureExt))
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
			i.Node.Stop();
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

	public McvNode RunNode(string net, IClock clock = null)
	{
		if(Nodes.Any(i => i.Net == net))
			throw new NodeException(NodeError.AlreadyRunning);

		var u = new Uri(Settings.Api.ListenAddress);
		var sh = $"{u.Scheme}://{u.Host}";
				
		var port = Enumerable.Range((int)Settings.Rdn.Zone + (int)KnownSystem.NodeApiPool, (int)KnownSystem.NodeApiPoolSize).Where(i => (i - Settings.NodesApiListenPortPostfix) % 10 == 0).First(i => Nodes.All(j => j.ApiPort != i));

		var api = new ApiSettings();
		
		api.ListenAddress = $"{sh}:{port}";
		api.AccessKey = Guid.NewGuid().ToString();

		McvNode n = null;

		if(Rdn.Rdn.Official.FirstOrDefault(i => i.Zone == Settings.Rdn.Zone) is Rdn.Rdn rdn && rdn.Name == net)
		{
			var f = Flow.CreateNested(net, new Log());

			f.WorkDirectory = Path.Join(Settings.Profile, rdn.Address);

			n = new RdnNode(Settings.Name, rdn, f.WorkDirectory, null, Settings.Packages, Settings.Api, api, clock, f);
		}
		else
		{
			var p = Path.Join(Path.GetDirectoryName(GetType().Assembly.Location), net + ".dll");
		
			if(p != null)
			{
				var a = Assembly.LoadFrom(p);

				var c = a.GetTypes().FirstOrDefault(i => i.IsSubclassOf(typeof(Node)))?
						 .GetConstructor([typeof(string), typeof(Zone), typeof(string), typeof(Settings), typeof(ApiSettings), typeof(ApiSettings), typeof(IClock), typeof(Flow)]);

				if(c != null)
				{
					var f = Flow.CreateNested(net, new Log());

					f.WorkDirectory = Path.Join(Settings.Profile, Net.Net.Escape(net));
	
					n = c.Invoke([Settings.Name, Settings.Rdn.Zone, f.WorkDirectory, null, Settings.Api, api, clock, f]) as McvNode;
				}
			}
		}

		if(n != null)
		{
			Nodes.Add(new NodeInstance {Net = net,
										ApiSettings = api,
										ApiPort = port,
										Node = n});
		
			NodeStarted?.Invoke(n);
		
			return n;
		} 
		else
			throw new NodeException(NodeError.NoNodeForNet);
	}

	public McvApiClient GetMcvNodeApi(string net)
	{
		var ni = Find(net);

		return new McvApiClient(ApiHttpClient, ni.ApiSettings.ListenAddress, ni.ApiSettings.AccessKey);
	}

	public override UosCommand Create(IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;

		var args = commnad.Skip(1).ToList();

		var ct = Assembly.GetExecutingAssembly().DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct.GetConstructor([typeof(Uos), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as UosCommand;
	}

  
  		//public Ura DeploymentToAddress(string path)
  		//{
  		//	return Ura.Parse(ResourceHub.Unescape(path.Substring(Settings.Packages.Length)));
  		//}

	public VersionManifest GetCurrentManifest(ApvAddress address)
	{
		var h = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, address), "." + VersionManifest.Extension);
		
		return File.Exists(h) ? VersionManifest.FromXon(new Xon(File.ReadAllText(h))) : null;
	}

	public void Start(Ura address, Flow flow)
	{
		var d = RdnApi.FindLocalResource(address, flow)?.Last
				?? 
				RdnApi.Request<ResourceResponse>(new PpcApc {Request = new ResourceRequest {Identifier = new (address)}}, flow)?.Resource?.Data;

		if(d == null)
			throw new UosException("Incorrect resource type");

		//Ura apr = null;
		Ura aprv = null;

		if(d.Type.Content == ContentType.Rdn_ProductManifest)
		{
			var lrr = RdnApi.Download(address, flow);

			var m = ProductManifest.FromXon(new Xon(new StreamReader(new MemoryStream(RdnApi.Request<byte[]>(new LocalReleaseReadApc {Address = lrr.Address, Path=""}, flow)), Encoding.UTF8).ReadToEnd()));

			aprv = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Latest;
		}
		else if(d.Type.Content == ContentType.Rdn_VersionManifest)
		{
			aprv = address;
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

	public void SetupApplicationEnvironemnt(Ura address)
	{
		Environment.SetEnvironmentVariable(Application.ApiAddressEnvKey,	Settings.Api.ListenAddress);
		Environment.SetEnvironmentVariable(Application.ApiKeyEnvKey,		Settings.Api.AccessKey);
		Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());
		Environment.SetEnvironmentVariable(Application.PackagesPathKey,		Settings.Packages);

		Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, address);
	}
}	
