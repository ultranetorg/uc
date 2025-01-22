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
							Description = "Sends multiple operations as one transaction or as few as possible",
							Syntax = $"{Keyword} name0={{operation}} name1={{operation}} ... nameN={{operation}} {SignerArg}={AA}",

							Arguments = [new ("name(n)", "Arbitrary name of operation, not used during processing"),
										 new ("operation", "Operation arguments"),
										 new (SignerArg, "Public address of account a transaction is sent on behalf of")],

							Examples = [new (null,  $"{Keyword} a={{account ut to={AA.Examples[1]} ec=5000}}" +
													$"			b={{account ut to={AA.Examples[2]} ec=5000}}" +
													$"			c={{account ut to={AA.Examples[3]} ec=5000}}" +
													$"			{SignerArg}={AA.Example}")]
						};

		a.Execute = () =>	{
								var results = Args	.Where(i =>	i.Name != AwaitArg && 
																i.Name != SignerArg)
													.Select(x => {
																	var op = (x.Value as Xon).Nodes;

																	var c = Cli.Create(op, Flow);

																	c.Args.RemoveAt(0);

																	var a = c.Actions.FirstOrDefault(i => i.Name == null || i.Names.Contains(op.Skip(1).First().Name));

																	return a.Execute();
																}
								);

								Transact(results.OfType<Operation>(), GetAccountAddress(SignerArg), GetAwaitStage(Args));

								return results;
							};

		return a;

	}
}
