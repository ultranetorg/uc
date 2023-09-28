using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class Program
	{
		static Settings			Settings = null;
		static Workflow			Workflow = new Workflow("CLI", new Log());
		static ConsoleLogView	LogView;
		static Net.Sun			Sun;
		internal static Boot	Boot;

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
					
				Boot = new Boot(exedir);
				Settings = new Settings(exedir, Boot);
								
				if(File.Exists(Settings.Profile))
					foreach(var i in Directory.EnumerateFiles(Settings.Profile, "*." + Net.Sun.FailureExt))
						File.Delete(i);

				if(!Boot.Commnand.Nodes.Any())
					return;

				//string dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

				Sun = new Net.Sun(Boot.Zone, Settings){	Clock = new RealTimeClock(),
														Nas = new Nas(Settings, Workflow.Log),
														GasAsker = Command.ConsoleAvailable ? new ConsoleGasAsker() : new SilentGasAsker(),
														FeeAsker = new SilentFeeAsker()};
				
				if(Boot.Commnand.Nodes.First().Name != RunCommand.Keyword)
				{
					Sun.RunUser(Workflow);
				}

				Execute(Boot.Commnand, Boot.Zone, Settings, Sun, Workflow);
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

			Sun.Stop("The End");
		}

		static Command Create(Zone zone, Settings settings, Net.Sun sun, Xon commnad, Workflow workflow)
		{
			Command c = null;
			var t = commnad.Nodes.First().Name;

			var args = new Xon {Nodes = commnad.Nodes.Skip(1).ToList()};

			switch(t)
			{
				case RunCommand.Keyword:		c = new RunCommand(zone, settings, workflow, sun, args); break;
				case DevCommand.Keyword:		c = new DevCommand(zone, settings, workflow, sun, args); break;
				case AccountCommand.Keyword:	c = new AccountCommand(zone, settings, workflow, sun, args); break;
				case UntCommand.Keyword:		c = new UntCommand(zone, settings, workflow, sun, args); break;
				case MembershipCommand.Keyword:	c = new MembershipCommand(zone, settings, workflow, sun, args); break;
				case AuthorCommand.Keyword:		c = new AuthorCommand(zone, settings, workflow, sun, args); break;
				case PackageCommand.Keyword:	c = new PackageCommand(zone, settings, workflow, sun, args); break;
				case ResourceCommand.Keyword:	c = new ResourceCommand(zone, settings, workflow, sun, args); break;
			}

			return c;
		}

		public static object Execute(Xon command, Zone zone, Settings settings, Net.Sun sun, Workflow workflow)
		{
			if(workflow.Aborted)
				throw new OperationCanceledException();

			var args = command.Nodes.ToList();
			var c = Create(zone, settings, sun, command, workflow);

			if(c != null)
			{
				var a = c?.Execute();

				if(a is Operation o)
				{
					c.Sun.Enqueue(new Operation[]{o}, c.Sun.Vault.GetKey(c.GetAccountAddress("by")), Command.GetAwaitStage(command),  workflow);
				}
				
				return a;
			} 
			else
			{
				var ops = command.Nodes.Where(i => i.Name != "await" && i.Name != "by").Select(i => Create(zone, settings, sun, i, workflow).Execute() as Operation).ToArray();

				sun.Enqueue(ops, sun.Vault.GetKey(AccountAddress.Parse(command.Get<string>("by"))), Command.GetAwaitStage(command), workflow);

				return ops;
			}
		}
	}
}
