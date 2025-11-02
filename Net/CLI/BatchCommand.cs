using System.Reflection;

namespace Uccs.Net;

public class BatchCommand : McvCommand
{
	public BatchCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Batch()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Help = new () { 
							Description = "Sends multiple operations as a single transaction",
							Syntax = $"{Keyword} name0={{OPERATION}} name1={{OPERATION}} ... nameN={{OPERATION}} {SignerArg}={AA}",

							Arguments = [new ("name(n)", NAME, "Arbitrary name of operation, not used during processing"),
										 new ("command", COMMAND, "Operation command arguments"),
										 SignerArgument()],

							Examples = [new (null,  $"{Keyword} a={{account ut to={AA.Examples[0]} ec={EC.Example}}}" +
													$"			b={{account ut to={AA.Examples[1]} ec={EC.Example}}}" +
													$"			c={{account ut to={AA.Examples[2]} ec={EC.Example}}}" +
													$"			{SignerArg}={AA.Example}")]
						};

		a.Execute = () =>	{
								return Args	.Where(i =>	i.Name != AORArg  && i.Name != SignerArg)
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
