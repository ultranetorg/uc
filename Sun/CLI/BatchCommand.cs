using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class BatchCommand : Command
	{
		public const string Keyword = "batch";

		public BatchCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =
			[
				new ()
				{
					Names = [],
					Help = new Help	{ 
										Title = "BATCH",
										Description = "Sends multiple operations as one transaction or as few as possible",
										Syntax = "batch name0={operation} name1={operation} ... nameN={operation} by=UAA",

										Arguments = [
														new ("name(n)", "Arbitrary name of operation, not used during processing"),
														new ("operation", "Operation arguments"),
														new ("by", "Public address of account a transaction is sent on behalf of")
													],

										Examples = [
														new (null, "batch a={money transfer to=0x1111111111111111111111111111111111111111 amount=5.000}" +
																  " b={domain bid company0 amount=1.000 by=0x0000fffb3f90771533b1739480987cee9f08d754}" +
																  " c={domain transfer company1 to=0x2222222222222222222222222222222222222222}" +
																  " by=0x0000fffb3f90771533b1739480987cee9f08d754")
													]
									},

					Execute = () =>	{
										var results = Args.Where(i => i.Name != "await" && i.Name != "by").Select(x => {
																															var c = Program.Create(x.Nodes, Flow);

																															var a = c.Actions.FirstOrDefault(i => !i.Names.Any() || i.Names.Contains(x.Nodes.Skip(1).First().Name));

																															return a.Execute();
																														}
										);

										Transact(results.OfType<Operation>(), GetAccountAddress("by"), GetAwaitStage(Args));

										return results;
									}
				}
			];	
		}
	}
}
