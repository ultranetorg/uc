using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// </summary>
	public class NexusCommand : Command
	{
		public const string Keyword = "nexus";

		public NexusCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "m" :
				case "membership" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var rp = Rdc(new MembersRequest());
	
					var m = rp.Members.FirstOrDefault(i => i.Account == AccountAddress.Parse(Args[1].Name));

					if(m == null)
						throw new EntityException(EntityError.NotFound);

					Dump(m);

					return m;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
