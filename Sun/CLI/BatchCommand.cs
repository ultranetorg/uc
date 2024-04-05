using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class BatchCommand : Command
	{
		public const string Keyword = "batch";

		public BatchCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			var results = Args.Nodes.Where(i => i.Name != "await" && i.Name != "by").Select(i => {
																									var c = Program.Create(i);
																									c.Workflow = Workflow;
																									return c;
																								 }).Select(i => i.Execute());

			Transact(results.OfType<Operation>(), GetAccountAddress("by"), Command.GetAwaitStage(Args));

			return results;
		}
	}
}
