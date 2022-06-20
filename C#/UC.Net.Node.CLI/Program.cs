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

namespace UC.Net.Node.CLI
{
	class Program
	{
		static Settings			Settings = null;
		static Log				Log = new Log();
		static ConsoleLogView	LogView = new ConsoleLogView(Log, false, true);

		static Core Core;


		static void Main(string[] args)
		{
			Thread.CurrentThread.CurrentCulture = 
			Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

			var exedir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

			try
			{
				foreach(var i in Directory.EnumerateFiles(exedir, "*." + Core.FailureExt))
					File.Delete(i);
					
				var b = new XonDocument(new XonTextReader(File.ReadAllText(Path.Combine(exedir, "Boot.xon"))), XonTextValueSerializator.Default);
				var cmd = new XonDocument(new XonTextReader(string.Join(' ', Environment.GetCommandLineArgs().Skip(1))), XonTextValueSerializator.Default);
				var boot = new BootArguments(b, cmd);

				var orig = Path.Combine(exedir, Core.SettingsFileName);
				var user = Path.Combine(boot.Main.Profile, Core.SettingsFileName);

				if(!File.Exists(user))
				{
					Directory.CreateDirectory(boot.Main.Profile);
					File.Copy(orig, user);
				}

				Log.Stream = new FileStream(Path.Combine(boot.Main.Profile, "Log.txt"), FileMode.Create);

				Settings = new Settings(boot);

				Cryptography.Current = Settings.Cryptography;
									
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Core.FailureExt))
						File.Delete(i);

				if(!cmd.Nodes.Any())
					return;


				string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Func<Core> d = () =>
									 {
									 	if(Core == null)
									 		Core = new Core(Settings, dir, Log)
													{
														Clock = new RealTimeClock(), 
														Nas = new Nas(Settings, Log), 
														GasAsker = new SilentGasAsker(Log), 
														FeeAsker = new SilentFeeAsker()
													}; 

									 	return Core;
									 };
				
				Command c = null;

				var t = cmd.Nodes.First().Name;

				cmd.Nodes.RemoveAt(0);

				switch(t)
				{
					case HostCommand.Keyword:		c = new HostCommand(Settings, Log, d, cmd); break;
					case NodeCommand.Keyword:		c = new NodeCommand(Settings, Log, d, cmd); break;
					case AccountCommand.Keyword:	c = new AccountCommand(Settings, Log, d, cmd); break;
					case UntCommand.Keyword:		c = new UntCommand(Settings, Log, d, cmd); break;
					case MembershipCommand.Keyword:	c = new MembershipCommand(Settings, Log, d, cmd); break;
					case AuthorCommand.Keyword:		c = new AuthorCommand(Settings, Log, d, cmd); break;
					case ProductCommand.Keyword:	c = new ProductCommand(Settings, Log, d, cmd); break;
				}

				try
				{
					c?.Execute();
				}
				catch(RpcException ex)
				{
					Log.ReportError(null, ex.Message);
				}
				catch(SyntaxException ex)
				{
					Log.ReportError(null, ex.Message);
				}
			}
			catch(AbortException)
			{
				Log.ReportError(null, "Execution aborted");
			}
			catch(Exception ex) when(!Debugger.IsAttached)
			{
				var m = Path.GetInvalidFileNameChars().Aggregate(MethodBase.GetCurrentMethod().Name, (c1, c2) => c1.Replace(c2, '_'));
				File.WriteAllText(Path.Join(Settings?.Profile ?? exedir, m + "." + Core.FailureExt), ex.ToString());
				Log.ReportError(null, $"{m} failed", ex);
			}
	
			if(Core != null)
			{
				Core.Stop("End");
			}
		}
	}
}
