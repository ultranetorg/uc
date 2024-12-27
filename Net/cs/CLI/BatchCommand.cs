namespace Uccs.Net;

public class BatchCommand : McvCommand
{
	public const string Keyword = "batch";

	public BatchCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =
		[
			new ()
			{
				Names = [],
				Help = new Help	{ 
									Title = "BATCH",
									Description = "Sends multiple operations as one transaction or as few as possible",
									Syntax = $"batch name0={{operation}} name1={{operation}} ... nameN={{operation}} signer=UAA",

									Arguments = [new ("name(n)", "Arbitrary name of operation, not used during processing"),
												 new ("operation", "Operation arguments"),
												 new (SignerArg, "Public address of account a transaction is sent on behalf of")],

									Examples = [new (null,	"batch a={account ut to=0x1111111111111111111111111111111111111111 ec=5000}" +
															" b={account ut to=0x1111111111111111111111111111111111111111 ec=5000}" +
															" c={account ut to=0x1111111111111111111111111111111111111111 ec=5000}" +
															" signer=0x0000fffb3f90771533b1739480987cee9f08d754")]
								},

				Execute = () =>	{
									var results = Args	.Where(i =>	i.Name != AwaitArg && 
																	i.Name != SignerArg)
														.Select(x => {
																		var c = Cli.Create(x.Nodes, Flow);

																		var a = c.Actions.FirstOrDefault(i => !i.Names.Any() || i.Names.Contains(x.Nodes.Skip(1).First().Name));

																		return a.Execute();
																	}
									);

									Transact(results.OfType<Operation>(), GetAccountAddress(SignerArg), GetAwaitStage(Args));

									return results;
								}
			}
		];	
	}
}
