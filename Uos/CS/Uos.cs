using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text.Json;
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
	 	
		public Net.Sun			Sun;
		public static bool		ConsoleAvailable { get; protected set; }
		public IPasswordAsker	PasswordAsker = new ConsolePasswordAsker();
		public Flow				Flow = new Flow("uos", new Log()); 
		public static string	ExeDirectory;
		public UosSettings		Settings;
		public List<Mcv>		Mcvs = [];
		public UosApiServer		ApiServer;
		public IClock			Clock;
		public Delegate			Stopped;
		public Vault			Vault;
		
		public Mcv				FindMcv(Guid id) => Mcvs.Find(i => i.Guid == id);
		public T				Find<T>() where T : Mcv => Mcvs.Find(i => i.GetType() == typeof(T)) as T;

		//public static List<Uos>			All = new();

		public SunDelegate		SunStarted;
		public McvDelegate		McvStarted;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var b = new Boot(ExeDirectory);
			var s = new UosSettings(b.Profile, "The Uos", b.Zone);
			
			var u = new Uos(s, new Flow("Uos", new Log()), new RealClock());
			u.Run();
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

			Vault = new Vault(Settings.Zone, Settings.Profile);

			//Environment.SetEnvironmentVariable(BootProductsPath,productspath);
			//Environment.SetEnvironmentVariable(BootSunAddress,	sunaddress);
			//Environment.SetEnvironmentVariable(BootSunApiKey,	sunapikey);
			//Environment.SetEnvironmentVariable(BootZone,		zone.Name);

			//ReportPreambule();
			//ReportNetwork();
			if(File.Exists(Settings.Profile))
				foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Net.Sun.FailureExt))
					File.Delete(i);
		
			
			if(Settings.Api != null)
			{
				RunApi();
			}
		}

		public override string ToString()
		{
			return string.Join(" - ", new string[]{ Settings.Name,
													ApiServer != null ? "A" : null, 
													Sun?.ToString(), 
													string.Join(" - ", Mcvs)}.Where(i => i != null));
		}

		public void Stop()
		{
			Flow.Abort();

			ApiServer?.Stop();

			foreach(var i in Mcvs.ToArray())
			{	
				lock(Sun.Lock)
					Sun.Disconnect(i);
		
				i.Stop();
				Mcvs.Remove(i);
			}

			Sun.Stop();
			Stopped?.Invoke(this);
		}

		public void Run()
		{
			Console.WriteLine();

			if(ConsoleAvailable)
				while(Flow.Active)
				{
					Console.Write("uos > ");
	
					try
					{
						var x = new XonDocument(Console.ReadLine());
	
						if(x.Nodes[0].Name == "sun")
						{
							var p = new Sun.CLI.Program(Sun, null, Flow, null);
							p.Execute(x.Nodes.Skip(1), Flow);
						}
					}
					catch(Exception ex)
					{
						Flow.Log.ReportError(this, "Error", ex);
					}
				}
			else
				WaitHandle.WaitAny([Flow.Cancellation.WaitHandle]);
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

		public void RunSun(SunSettings settings = null)
		{
			var s = settings ?? new SunSettings(Settings.Profile);

			Sun = new Net.Sun(s, Settings.Zone, Vault, Flow);

			SunStarted?.Invoke(Sun);
		}

		public void RunMcv(Guid mcvid, McvSettings settings = null, IEthereum ethereum = null, IClock clock = null)
		{
			if(Rds.Id == mcvid)
			{
				var m = new Rds(Sun, 
								settings as RdsSettings ?? new RdsSettings(Settings.Profile), 
								Path.Join(Settings.Profile, Rds.Id.ToString()),
								Flow.CreateNested(nameof(Rds), Flow.Log),
								ethereum ?? new Ethereum(settings as RdsSettings ?? new RdsSettings(Settings.Profile)),
								clock ?? new RealClock());
				
				Mcvs.Add(m);

				lock(Sun.Lock)
					Sun.Connect(m);

				McvStarted?.Invoke(m);
			}
		}

		public UosCommand Create(IEnumerable<Xon> commnad, Flow flow)
		{
			UosCommand c;
			var t = commnad.First().Name;

			var args = commnad.Skip(1).ToList();

			switch(t)
			{
				case WalletCommand.Keyword:		c = new WalletCommand(this, args, flow); break;

				default:
					throw new SyntaxException("Unknown command");
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
    }
}
