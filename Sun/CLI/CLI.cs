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
					
				var boot = new Boot(exedir);

				Settings = new Settings(exedir, boot);
								
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Core.FailureExt))
						File.Delete(i);

				if(!boot.Commnand.Nodes.Any())
					return;

				//string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Func<Core> getcore = () =>	{
												if(Core == null)
												{
													Core =	new Core(boot.Zone, Settings, Log)	
															{
																Clock = new RealTimeClock(),
																Nas = new Nas(Settings, Log),
																GasAsker = new SilentGasAsker(Log),
																FeeAsker = new SilentFeeAsker()
															};
												}

												return Core;
											};


				Func<Core> getuser = () =>	{
												if(Core == null)
												{
													Core =	new Core(boot.Zone, Settings, Log)	
															{
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

				var t = boot.Commnand.Nodes.First().Name;

				boot.Commnand.Nodes.RemoveAt(0);

				switch(t)
				{
					case RunCommand.Keyword:			c = new RunCommand(boot.Zone, Settings, Log, getcore, boot.Commnand); break;
					case DevCommand.Keyword:			c = new DevCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case AccountCommand.Keyword:		c = new AccountCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case UntCommand.Keyword:			c = new UntCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case MembershipCommand.Keyword:		c = new MembershipCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case AuthorCommand.Keyword:			c = new AuthorCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case ProductCommand.Keyword:		c = new ProductCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case RealizationCommand.Keyword:	c = new RealizationCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
					case ReleaseCommand.Keyword:		c = new ReleaseCommand(boot.Zone, Settings, Log, getuser, boot.Commnand); break;
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
