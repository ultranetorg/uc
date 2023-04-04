using Nethereum.Signer;
using Nethereum.Web3;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	class Program
	{
		static Settings			Settings = null;
		static Log				Log = new Log();
		static ConsoleLogView	LogView;

		static Core Core;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			try
			{
				var p = Console.KeyAvailable;
				LogView = new ConsoleLogView(Log, true, true);
			}
			catch(Exception)
			{
			}

			try
			{
				foreach(var i in Directory.EnumerateFiles(exedir, "*." + Core.FailureExt))
					File.Delete(i);
					
				var b = new XonDocument(File.ReadAllText(Path.Combine(exedir, "Boot.xon")));
				var cmd = new XonDocument(string.Join(' ', Environment.GetCommandLineArgs().Skip(1)));
				var boot = new BootArguments(b, cmd);

				Settings = new Settings(exedir, boot);
								
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Core.FailureExt))
						File.Delete(i);

				if(!cmd.Nodes.Any())
					return;

				//string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Func<Core> getcore =	() =>
										{
											if(Core == null)
											{
												Core = new Core(Settings, exedir, Log)	{
																							Clock = new RealTimeClock(),
																							Nas = new Nas(Settings, Log),
																							GasAsker = new SilentGasAsker(Log),
																							FeeAsker = new SilentFeeAsker()
																						};
											}

											return Core;
										};


				Func<Core> getuser =	() =>
										{
											if(Core == null)
											{
												Core = new Core(Settings, exedir, Log)	{
																							Clock = new RealTimeClock(),
																							Nas = new Nas(Settings, Log),
																							GasAsker = new SilentGasAsker(Log),
																							FeeAsker = new SilentFeeAsker()
																						};

												Core.RunUser();
											}

											return Core;
										};
				
				Command c = null;

				var t = cmd.Nodes.First().Name;

				cmd.Nodes.RemoveAt(0);

				switch(t)
				{
					case RunCommand.Keyword:			c = new RunCommand(Settings, Log, getcore, cmd); break;
					case DevCommand.Keyword:			c = new DevCommand(Settings, Log, getuser, cmd); break;
					case AccountCommand.Keyword:		c = new AccountCommand(Settings, Log, getuser, cmd); break;
					case UntCommand.Keyword:			c = new UntCommand(Settings, Log, getuser, cmd); break;
					case MembershipCommand.Keyword:		c = new MembershipCommand(Settings, Log, getuser, cmd); break;
					case AuthorCommand.Keyword:			c = new AuthorCommand(Settings, Log, getuser, cmd); break;
					case ProductCommand.Keyword:		c = new ProductCommand(Settings, Log, getuser, cmd); break;
					case RealizationCommand.Keyword:	c = new RealizationCommand(Settings, Log, getuser, cmd); break;
					case ReleaseCommand.Keyword:		c = new ReleaseCommand(Settings, Log, getuser, cmd); break;
				}

				c?.Execute();
			}
			catch(AbortException)
			{
				Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings?.Profile ?? exedir, m + "." + Core.FailureExt), ex.ToString());
				throw;
			}
	
			if(Core != null)
			{
				Core.Stop("The End");
			}
		}
	}
}
