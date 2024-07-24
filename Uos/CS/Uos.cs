using System.Net;
using System.Reflection;
using Uccs.Net;

namespace Uccs.Uos
{
	public class Uos
	{
		public const string BootProductsPath	= "Uos.Nexus.ProductsPath";
		public const string BootSunAddress 		= "Uos.Nexus.SunAddress";
		public const string BootSunApiKey		= "Uos.Nexus.SunApiKey";
		public const string BootZone		 	= "Uos.Nexus.Zone";

		public delegate void	Delegate(Uos d);
 		public delegate void	McvDelegate(Mcv d);
	 	
		public InterzoneNode	Izn; /// Inter-consensus node
		public static bool		ConsoleAvailable { get; protected set; }
		public IPasswordAsker	PasswordAsker = new ConsolePasswordAsker();
		public Flow				Flow = new Flow("uos", new Log()); 
		public static string	ExeDirectory;
		public UosSettings		Settings;
		public List<Node>		Nodes = [];
		public UosApiServer		ApiServer;
		public IClock			Clock;
		public Delegate			Stopped;
		public Vault			Vault;
		static Boot				Boot;
		public static			ConsoleLogView	LogView = new ConsoleLogView(false, false);
		
		public Node				Find(Guid id) => Nodes.Find(i => i.Zone.Id == id);
		public T				Find<T>() where T : Node => Nodes.Find(i => i.GetType() == typeof(T)) as T;

		//public static List<Uos>			All = new();

		public NodeDelegate		IcnStarted;
		public NodeDelegate		McvStarted;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			Boot = new Boot(ExeDirectory);
			var s = new UosSettings(Boot.Profile, "The Uos", Interzone.ByName(Boot.Zone));
			
			var u = new Uos(s, new Flow("Uos", new Log()), new RealClock());
			u.Execute(Boot.Commnand.Nodes, u.Flow);
			u.Stop();
		}

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
			//Environment.SetEnvironmentVariable(BootZone,		zone.Name);

			//ReportPreambule();
			//ReportNetwork();
			if(File.Exists(Settings.Profile))
				foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Net.Node.FailureExt))
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
					Izn.Disconnect(i);
		
				i.Stop();
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

			Izn = new InterzoneNode(Settings.Name, Settings.Interzone.Id, Settings.Profile, settings, f);

			IcnStarted?.Invoke(Izn);
		}

		public Node RunNode(Guid zuid, NodeSettings settings = null, IClock clock = null, bool peering = false)
		{
			if(	RdnZone.Local.Id		== zuid ||
				RdnZone.Developer0.Id	== zuid ||
				RdnZone.Test.Id			== zuid)
			{
				var f = Flow.CreateNested(nameof(Rdn), new Log());

				var n = new RdnNode(Settings.Name, zuid, Settings.Profile, settings as RdnSettings, Vault, clock, f);

				Nodes.Add(n);

				lock(Izn.Lock)
					Izn.Connect(n);

				McvStarted?.Invoke(n);

				return n;
			}

			throw new NodeException(NodeError.NoNodeForZone);
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
				case AprvCommand.Keyword:		c = new AprvCommand(this, args, flow); break;

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

 		public string AddressToDeployment(AprvAddress resource)
 		{
 			return Path.Join(Settings.Packages, ResourceHub.Escape(resource.APR), ResourceHub.Escape(resource.Version));
 		}
  
  		//public Ura DeploymentToAddress(string path)
  		//{
  		//	return Ura.Parse(ResourceHub.Unescape(path.Substring(Settings.Packages.Length)));
  		//}

		public byte[] GetCurrentHash(AprvAddress address)
		{
			var h = Path.Join(AddressToDeployment(address), ".hash");
			
			return File.Exists(h) ? File.ReadAllText(h).FromHex() : null;
		}

		public void Install(AprvAddress address, Flow flow)
		{
			var h = new HttpClientHandler();
			h.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
			var c = new HttpClient(h) {Timeout = Timeout.InfiniteTimeSpan};

			var a = new RdnApiClient(c, Find<RdnNode>().Settings.Api.ListenAddress, Find<RdnNode>().Settings.Api.AccessKey);

			var p = a.Request<PackageInfo>(new PackageInfoApc {Package = address}, flow);

			if(p == null || !p.Manifest.CompleteHash.SequenceEqual(GetCurrentHash(address)))
			{
				if(p == null || !p.Ready)
				{
					a.Send(new PackageDownloadApc {Package = address}, flow);
	
					do
					{
						var d = a.Request<ResourceActivityProgress>(new PackageActivityProgressApc {Package = address}, flow);
			
						if(d is null)
						{	
							if(!a.Request<PackageInfo>(new PackageInfoApc {Package = address}, flow).Ready)
							{
								flow.Log?.ReportError(this, "Failed");
								return;
							}
							else
								break;
						}
	
						Thread.Sleep(100);
					}
					while(Flow.Active);
				}

				Find<RdnNode>().PackageHub.Deploy(address, a => AddressToDeployment(new AprvAddress(a)), flow);

				do
				{
					var d = a.Request<ResourceActivityProgress>(new PackageActivityProgressApc {Package = address}, flow);
			
					if(d is null)
					{	
						if(!a.Request<PackageInfo>(new PackageInfoApc {Package = address}, flow).Ready)
						{
							flow.Log?.ReportError(this, "Failed");
							return;
						}
						else
							break;
					}
	
					Thread.Sleep(100);
				}
				while(Flow.Active);
			}
			
		}
    }
}
