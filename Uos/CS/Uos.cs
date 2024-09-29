using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;
using Uccs.Net;

namespace Uccs.Uos
{
	public class NodeInstance
	{
		public ApiSettings	Api { get; set; }
		public Node			Node;
		public Guid			Id;
		RdnApiClient		_Rdn;

		public RdnApiClient Rdn
		{
  			get
  			{
				return _Rdn ??= new RdnApiClient(Uos.ApiHttpClient, Api.ListenAddress, Api.AccessKey);
  			}
		}

		public override string ToString()
		{
			return Node.ToString();
		}
	}

	public class Uos
	{
		public delegate void		Delegate(Uos d);
 		public delegate void		McvDelegate(Mcv d);
	 	
		public NexusNode		Izn; /// Inter-consensus node
		public static bool			ConsoleAvailable { get; protected set; }
		public IPasswordAsker		PasswordAsker = new ConsolePasswordAsker();
		public Flow					Flow = new Flow("uos", new Log()); 
		public static string		ExeDirectory;
		public UosSettings			Settings;
		public List<NodeInstance>	Nodes = [];
		public UosApiServer			ApiServer;
		public IClock				Clock;
		public Delegate				Stopped;
		public Vault				Vault;
		static Boot					Boot;
		public static				ConsoleLogView	LogView = new ConsoleLogView(false, false);
		public static HttpClient	ApiHttpClient;

		public Node					Find(Guid id) => Nodes.Find(i => i.Id == id)?.Node;
		public T					Find<T>() where T : Node => Nodes.Find(i => i.Node.GetType() == typeof(T))?.Node as T;

		public RdnApiClient			Rdn => Nodes.Find(i => i.Id == Settings.Nexus.DefaultRdn).Rdn;
		//public static List<Uos>			All = new();

		public NodeDelegate			IznStarted;
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
			var s = new UosSettings(Boot.Profile, "The Uos", Nexus.ByName(Boot.Net));
			
			var u = new Uos(s, new Flow("Uos", new Log()), new RealClock());
			u.Execute(Boot.Commnand.Nodes, u.Flow);
			u.Stop();
		}

		public Uos(UosSettings settings, Flow flow, IClock clock)
		{
			Settings = settings;
			Flow = flow;
			Clock = clock;

			Settings.Packages ??= Path.Join(Settings.Profile, nameof(Settings.Packages));
			Directory.CreateDirectory(Settings.Packages);

			Vault = new Vault(Settings.Profile, settings.EncryptVault);

			//Environment.SetEnvironmentVariable(BootProductsPath,productspath);
			//Environment.SetEnvironmentVariable(BootSunAddress,	sunaddress);
			//Environment.SetEnvironmentVariable(BootSunApiKey,	sunapikey);
			//Environment.SetEnvironmentVariable(BootNet,		net.Name);

			//ReportPreambule();
			//ReportNetwork();
			if(File.Exists(Settings.Profile))
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
													Izn?.ToString()}.Where(i => i != null));
		}

		public void Stop()
		{
			Flow.Abort();

			Stopped?.Invoke(this);

			ApiServer?.Stop();

			foreach(var i in Nodes.ToArray())
			{	
				lock(Izn.Lock)
					Izn.Disconnect(i.Node);
		
				i.Node.Stop();
				Nodes.Remove(i);
			}

			Izn?.Stop();
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

		public void RunIcn(IznSettings settings = null)
		{
			var f = Flow.CreateNested(nameof(Izn), new Log());

			Izn = new NexusNode(Settings.Name, Settings.Nexus.Id, Settings.Profile, settings, f);

			IznStarted?.Invoke(Izn);
		}

		public Node RunNode(Guid id, NodeSettings settings = null, IClock clock = null, bool peering = false)
		{
			if(Settings.Nexus.DefaultRdn == id)
			{
				var f = Flow.CreateNested(nameof(Rdn), new Log());

				var n = new RdnNode(Settings.Name, id, Settings.Profile, settings as RdnSettings, Settings.Packages, Vault, clock, f);

				Nodes.Add(new NodeInstance {Id = id,
											Api = n.Settings.Api,
											Node = n});

				lock(Izn.Lock)
					Izn.Connect(n);

				NodeStarted?.Invoke(n);

				return n;
			}

			throw new NodeException(NodeError.NoNodeForNet);
		}

		public UosCommand Create(IEnumerable<Xon> commnad, Flow flow)
		{
			UosCommand c;
			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			switch(t)
			{
				case WalletCommand.Keyword:		c = new WalletCommand(this, args, flow); break;
				case NodeCommand.Keyword:		c = new NodeCommand(this, args, flow); break;
				case StartCommand.Keyword:		c = new StartCommand(this, args, flow); break;
				case PackageCommand.Keyword:	c = new PackageCommand(this, args, flow); break;

				default:
					throw new SyntaxException($"Unknown command '{t}'");
			}

			return c;
		}

		public object Execute(IEnumerable<Xon> command, Flow flow)
		{
			if(Flow.Aborted)
				throw new OperationCanceledException();

			if(command.Skip(1).FirstOrDefault()?.Name == "?")
			{
				var l = new Log();
				var v = new ConsoleLogView(false, false);
				v.StartListening(l);

				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.IsSubclassOf(typeof(Command)) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, flow]) as UosCommand;

				foreach(var j in c.Actions)
				{
					c.Report(string.Join(", ", j.Names));
					c.Report("");
					c.Report("   Syntax      : "+ j.Help?.Syntax);
					c.Report("   Description : "+ j.Help?.Description);
					c.Report("");
				}

				return c;
			}
			else if(command.Skip(2).FirstOrDefault()?.Name == "?")
			{
				var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.IsSubclassOf(typeof(Command)) && i.Name.ToLower() == command.First().Name + "command");

				var c = Activator.CreateInstance(t, [this, null, flow]) as UosCommand;

				var a = c.Actions.FirstOrDefault(i => i.Names.Contains(command.Skip(1).First().Name));

				c.Report("Syntax :");
				c.Report("");
				c.Report("   " + a.Help.Syntax);

				c.Report("");

				c.Report("Description :");
				c.Report("");
				c.Report("   " + a.Help.Description);

				if(a.Help.Arguments.Any())
				{ 
					c.Report("");

					c.Report("Arguments :");
					c.Report("");
					c.Dump(a.Help.Arguments, ["Name", "Description"], [i => i.Name, i => i.Description], 1);
				}
									
				return c;
			}
			else
			{
				var c = Create(command, flow);

				var a = c.Actions.FirstOrDefault(i => i.Names.Length == 0 || i.Names.Contains(command.Skip(1).FirstOrDefault()?.Name));

				if(a.Names.Any())
				{
					c.Args.RemoveAt(0);
				}

				var r = a.Execute();
				
				return r;
			}
		}
  
  		//public Ura DeploymentToAddress(string path)
  		//{
  		//	return Ura.Parse(ResourceHub.Unescape(path.Substring(Settings.Packages.Length)));
  		//}

		public VersionManifest GetCurrentManifest(AprvAddress address)
		{
			var h = Path.Join(PackageHub.AddressToDeployment(Settings.Packages, address), "." + VersionManifest.Extension);
			
			return File.Exists(h) ? VersionManifest.FromXon(new Xon(File.ReadAllText(h))) : null;
		}

		public void Start(Ura address, Flow flow)
		{
			var rdn = Nodes.Find(i => i.Id == Settings.Nexus.DefaultRdn).Rdn;

			var d = rdn.FindLocalResource(address, flow)?.Last
					?? 
					rdn.Request<ResourceResponse>(new PeerRequestApc {Request = new ResourceRequest {Identifier = new (address)}}, flow)?.Resource?.Data;

			if(d == null)
				throw new UosException("Incorrent resource type");

			Ura apr = null;
			AprvAddress aprv = null;

			if(d.Type.Content == ContentType.Rdn_ProductManifest)
			{
				var lrr = rdn.Download(address, flow);

				var m = ProductManifest.FromXon(new Xon(new StreamReader(new MemoryStream(rdn.Request<byte[]>(new LocalReleaseReadApc {Address = lrr.Address, Path=""}, flow)), Encoding.UTF8).ReadToEnd()));

				apr = m.Realizations.FirstOrDefault(i => i.Condition.Match(Platform.Current)).Address;
			}
			else if(d.Type.Control == DataType.Redirect_ProductRealization)
			{
				aprv = d.Parse<AprvAddress>();
			}
			else if(d.Type.Content == ContentType.Rdn_PackageManifest)
			{
				aprv = new (address);
			}
			else
				throw new UosException("Incorrent resource type");

			if(aprv == null)
			{
				d = rdn.Request<ResourceResponse>(new PeerRequestApc {Request = new ResourceRequest {Identifier = new (apr)}}, flow).Resource?.Data;
				aprv = d.Parse<AprvAddress>();
			}

			rdn.DeployPackage(aprv, Settings.Packages, flow);

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

		public void SetupApplicationEnvironemnt(AprvAddress address)
		{
			Environment.SetEnvironmentVariable(Application.ApiAddressEnvKey,	Settings.Api.ListenAddress);
			Environment.SetEnvironmentVariable(Application.ApiKeyEnvKey,		Settings.Api.AccessKey);
			Environment.SetEnvironmentVariable(Application.PackageAddressKey,	address.ToString());
			Environment.SetEnvironmentVariable(Application.PackagesPathKey,		Settings.Packages);

			Environment.CurrentDirectory = PackageHub.AddressToDeployment(Settings.Packages, new AprvAddress(address));
		}
    }
}
