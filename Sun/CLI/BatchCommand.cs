using System;
using System.IO;
using System.Linq;
using System.Threading;
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
			var results = Args.Nodes.Where(i => i.Name != "await" && i.Name != "by").Select(i => Program.Create(i)).Select(i => i.Execute());

			Program.Enqueue(results.OfType<Operation>(), GetAccountAddress("by"), Command.GetAwaitStage(Args));

			return results;
		}
	}
}
