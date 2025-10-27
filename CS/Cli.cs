using System.Diagnostics;
using System.Reflection;

namespace Uccs;

public abstract class Cli
{
	public const string			FailureExt = "failure";

	public static string		ExeDirectory;
	public ConsoleLogView		LogView = new ConsoleLogView(false, true);
	public Flow					Flow; 

	public abstract Command		Create(IEnumerable<Xon> commnad, Flow flow);

	public static bool			ConsoleAvailable { get; protected set; }

	static Cli()
	{
		Thread.CurrentThread.CurrentCulture = 
		Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

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

	protected Cli()
	{
	}

	protected void Execute(Boot boot)
	{
		if(!boot.Commnand.Nodes.Any())
			return;

		Flow = new Flow(GetType().Name, new Log()); 
			
		try
		{
			var l = new Log();
			LogView.StartListening(l);

			Execute(boot.Commnand.Nodes, Flow.CreateNested("Command", l));
		}
		catch(OperationCanceledException)
		{
			Flow.Log.ReportError(null, "Execution aborted");
		}
		catch(Exception ex) when(!Debugger.IsAttached)
		{
			if(Command.ConsoleAvailable)
			{
				Console.WriteLine(ex.ToString());

				if(ex is ApiCallException ace)
				{
					Console.WriteLine(ace.Response.Content.ReadAsStringAsync().Result);
				}
			}

			Directory.CreateDirectory(boot.Profile);
			File.WriteAllText(Path.Join(boot.Profile, $"{Flow.Name}.{FailureExt}"), ex.ToString());
		}

		LogView.StopListening();
	}

	public object Execute(IEnumerable<Xon> args, Flow flow)
	{
		if(flow.Aborted)
			throw new OperationCanceledException();

		if(args.Skip(1).FirstOrDefault()?.Name == "?")
		{
			var l = new Log();
			var v = new ConsoleLogView(false, false);
			v.StartListening(l);

			var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(Command) && i.Name.ToLower() == args.First().Name + nameof(Command).ToLower());

			var c = Activator.CreateInstance(t, [this, null, flow]) as Command;

			foreach(var i in c.Actions)
			{
				c.Report(string.Join(", ", i.Names));
				c.Report("");
				c.Report("   Syntax      : " + i.Help?.Syntax);
				c.Report("   Description : " + i.Help?.Description);
				c.Report("");
			}

			return c;
		}
		else if(args.Skip(2).FirstOrDefault()?.Name == "?")
		{
			var t = GetType().Assembly.GetTypes().FirstOrDefault(i => i.BaseType == typeof(Command) && i.Name.ToLower() == args.First().Name + "command");

			var c = Activator.CreateInstance(t, [this, null, flow]) as Command;

			var a = c.Actions.FirstOrDefault(i => i.Names.Contains(args.Skip(1).First().Name));

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
				c.Flow.Log?.Dump(a.Help.Arguments, ["Name", "Description"], [i => i.Name, i => i.Description], 1);
			}
								
			return c;
		}
		else
		{
			var c = Create(args, flow);

			var a = c.Actions.FirstOrDefault(i => i.Name == null || i.Names.Contains(args.Skip(1).FirstOrDefault()?.Name));

			if(a.Name != null)
			{
				c.Args.RemoveAt(0);
			}

			var r = a.Execute();

			PostExecute(args, c, r, flow);
			
			return r;
		}
	}

	public virtual void	 PostExecute(IEnumerable<Xon> args, Command command, object result, Flow flow)
	{
	}
}
