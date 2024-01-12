using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// </summary>
	public class MemberCommand : Command
	{
		public const string Keyword = "nexus";

		public MemberCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
				case "e" :
				case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var rp = Rdc<MembersResponse>(new MembersRequest());
	
					var m = rp.Members.FirstOrDefault(i => i.Account == AccountAddress.Parse(Args.Nodes[1].Name));

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
