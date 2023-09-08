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
		static Workflow			Workflow = new Workflow("CLI", new Log());
		static ConsoleLogView	LogView;
		static Net.Sun			Sun;

		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			try
			{
				var p = Console.KeyAvailable;
				LogView = new ConsoleLogView(Workflow.Log, true, true);
			}
			catch(Exception)
			{
			}

			try
			{
				foreach(var i in Directory.EnumerateFiles(exedir, "*." + Net.Sun.FailureExt))
					File.Delete(i);
					
				var boot = new Boot(exedir);

				Settings = new Settings(exedir, boot);
								
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Net.Sun.FailureExt))
						File.Delete(i);

				if(!boot.Commnand.Nodes.Any())
					return;

				//string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Func<Net.Sun> getsun = () =>	{
													if(Sun == null)
													{
														Sun = new Net.Sun(boot.Zone, Settings){
																									Clock = new RealTimeClock(),
																									Nas = new Nas(Settings, Workflow.Log),
																									GasAsker = new SilentGasAsker(),
																									FeeAsker = new SilentFeeAsker()
																								};
													}

													return Sun;
												};


				Func<Net.Sun> getuser = () =>	{
													if(Sun == null)
													{
														Sun = getsun();
														Sun.RunUser(Workflow);
													}

													return Sun;
												};
				
				Command c = null;

				var t = boot.Commnand.Nodes.First().Name;

				boot.Commnand.Nodes.RemoveAt(0);

				var w = Workflow.CreateNested("Command");

				switch(t)
				{
					case RunCommand.Keyword:			c = new RunCommand(boot.Zone, Settings, w, getsun, boot.Commnand); break;
					case DevCommand.Keyword:			c = new DevCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
					case AccountCommand.Keyword:		c = new AccountCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
					case UntCommand.Keyword:			c = new UntCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
					case MembershipCommand.Keyword:		c = new MembershipCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
					case AuthorCommand.Keyword:			c = new AuthorCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
					case PackageCommand.Keyword:		c = new PackageCommand(boot.Zone, Settings, w, getuser, boot.Commnand); break;
				}

				c?.Execute();
			}
			catch(AbortException)
			{
				Workflow.Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings?.Profile ?? exedir, m + "." + Net.Sun.FailureExt), ex.ToString());
				throw;
			}
	
			if(Sun != null)
			{
				Sun.Stop("The End");
			}
		}
	}
}
