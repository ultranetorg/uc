namespace Uccs.Net;

public abstract class Cli
{
	public abstract Command		Create(IEnumerable<Xon> commnad, Flow flow);

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
				c.Dump(a.Help.Arguments, ["Name", "Description"], [i => i.Name, i => i.Description], 1);
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

			PostExecute(args, c, r);
			
			return r;
		}
	}

	public virtual void	 PostExecute(IEnumerable<Xon> args, Command command, object result)
	{
	}
}
