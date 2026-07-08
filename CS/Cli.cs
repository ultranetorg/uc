using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;

namespace Uccs;

public abstract class Cli
{
	public const string			FailureExt = "failure";

	public string				Application;
	public static string		ExeDirectory;
	public ConsoleLogView		LogView = new ConsoleLogView(false, true);

	public static bool			ConsoleAvailable { get; protected set; }

	static Cli()
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

	protected Cli()
	{
		Application = Assembly.GetEntryAssembly().Location;
	}

	public virtual Command Create(IEnumerable<Xon> commnad, Flow flow)
	{
		return null;
	}

	public Command CreateFromAssembly(Assembly assembly, IEnumerable<Xon> commnad, Flow flow)
	{
		var t = commnad.First().Name;
		var args = commnad.Skip(1).ToList();
		var ct = assembly.DefinedTypes.Where(i => i.IsSubclassOf(typeof(Command))).FirstOrDefault(i => i.Name.ToLower() == t + nameof(Command).ToLower());

		return ct?.GetConstructor([GetType(), typeof(List<Xon>), typeof(Flow)]).Invoke([this, args, flow]) as Command;
	}

	public void Execute(string profile, Xon command)
	{
		var l = new Log();
		var f = new Flow(GetType().Name, l){WorkDirectory = profile}; 
			
		LogView.StartListening(l);

		try
		{
			if(!command.Nodes.Any())
				throw new SyntaxException("Nothing to do");

			Execute(command.Nodes, f);
		}
		catch(OperationCanceledException)
		{
			f.Log.ReportError(null, "Execution aborted");
		}
		catch(SyntaxException ex)
		{
			f.Log.ReportError(ex.Message);
		}
		catch(ApiCallException ex) when(ex.InnerException is HttpRequestException hre && hre.InnerException is SocketException)
		{
			f.Log.ReportError(null, "Unable to connect to Uos Nexus", ex);
		}
		catch(Exception ex) when(!Debugger.IsAttached)
		{
			if(Command.ConsoleAvailable)
			{
				f.Log.ReportError(ex.Message);

				#if DEBUG
				if(ex is ApiCallException ace && ace.Response?.Content != null)
				{
					Console.WriteLine("API call response:");
					Console.WriteLine(ace.Response.Content.ReadAsStringAsync().Result);
				}
				#endif
			}

			Directory.CreateDirectory(profile);
			File.WriteAllText(Path.Join(profile, $"{f.Name}.{FailureExt}"), ex.ToString());
		}

		LogView.StopListening();
	}

	public virtual void InteractOrWait(string profile, Command command, Command.CommandAction action, Flow flow)
	{
		if(ConsoleAvailable)
		{
			while(flow.Active)
			{
				Console.Write($"> ");

				var x = new Xon(Console.ReadLine());

				var first = x.Nodes.FirstOrDefault()?.Name;

				if(first == "exit")
					return;

				if(first == command.Keyword)
				{	
					Console.WriteLine("Already here");
					continue;
				}

				Execute(profile, x);
			}
		}
		else
			flow.Cancellation.WaitHandle.WaitOne();

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
				c.Report("   Syntax      : " + i.Syntax);
				c.Report("   Description : " + i.Description);
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
			c.Report("   " + a.Syntax);

			c.Report("");

			c.Report("Description :");
			c.Report("");
			c.Report("   " + a.Description);

			if(a.Arguments.Any())
			{ 
				c.Report("");

				c.Report("Arguments :");
				c.Report("");
				c.Flow.Log?.Dump(a.Arguments, ["Name", "Description"], [i => i.Name, i => i.Description], 1);
			}
								
			return c;
		}
		else
		{
			var c = Create(args, flow)
					??
					throw new SyntaxException("Unknown command name");

			var a = c.Actions.FirstOrDefault(i => i.Name == null || i.Names.Contains(args.Skip(1).FirstOrDefault()?.Name))
					??
					throw new SyntaxException("Unknown or missing command action");

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
