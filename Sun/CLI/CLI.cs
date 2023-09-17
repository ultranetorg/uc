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
														Sun = new Net.Sun(boot.Zone, Settings){	Clock = new RealTimeClock(),
																								Nas = new Nas(Settings, Workflow.Log),
																								GasAsker = new SilentGasAsker(),
																								FeeAsker = new SilentFeeAsker()};
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
				
				var w = Workflow.CreateNested("Command");

				Execute(boot.Commnand, boot.Zone, Settings, w, boot.Commnand.Nodes.First().Name == RunCommand.Keyword ? getsun : getuser);
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

		static Command Create(Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> sun, Xon commnad)
		{
			Command c = null;
			var t = commnad.Nodes.First().Name;

			var args = new Xon {Nodes = commnad.Nodes.Skip(1).ToList() };

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

		public static object Execute(Xon command, Zone zone, Settings settings, Workflow workflow, Func<Net.Sun> getsun)
		{
			var args = command.Nodes.ToList();
			var c = Create(zone, settings, workflow, getsun, command);

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
				var sun = getsun();

				var ops = command.Nodes.Where(i => i.Name != "await" && i.Name != "by").Select(i => Create(zone, settings, workflow, getsun, i).Execute() as Operation).ToArray();

				sun.Enqueue(ops, sun.Vault.GetKey(AccountAddress.Parse(command.Get<string>("by"))), Command.GetAwaitStage(command), workflow);

				return ops;
			}
		}
	}
}
