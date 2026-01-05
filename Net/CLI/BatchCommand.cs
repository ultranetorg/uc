using System.Reflection;

namespace Uccs.Net;

public class BatchCommand : McvCommand
{
	public BatchCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Batch()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Sends multiple operations as a single transaction";
		a.Arguments = [new ("command", COMMAND, "Operation command arguments", Flag.Multi),
						ByArgument()];

		a.Execute = () =>	{
								return Args	.Where(i =>	i.Name != AORArg  && i.Name != ByArg)
											.Select(x => {
															var op = (x.Value as Xon).Nodes;

															var c = Cli.Create(op, Flow);

															c.Args.RemoveAt(0);

															var a = c.Actions.FirstOrDefault(i => i.Name == null || i.Names.Contains(op.Skip(1).First().Name));

															var o = a.Execute() as Operation;

															if(o is null)
																throw new SyntaxException($"{c.Keyword} is not operation");

															return o;
														});

							};

		return a;

	}
}
