using System.Linq;
using System.Net;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: ultranode declare
	///						candidate = ACCOUNT 
	///						[password = PASSWORD] 
	///						bail = UNT 
	///						ip = IP
	///							
	///	Example: membership declare candidate=0x0007f34bc43d41cf3ec2e6f684c7b9b131b04b41 bail=1000 ip=192.168.1.107
	/// </summary>
	public class NexusCommand : Command
	{
		public const string Keyword = "nexus";

		public NexusCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		//case "declare" : 
				//	return new CandidacyDeclaration(Money.ParseDecimal(GetString("bail")), 
				//									GetString("baseips").Split(' ').Select(i => IPAddress.Parse(i)).ToArray(),
				//									GetString("seedhubips").Split(' ').Select(i => IPAddress.Parse(i)).ToArray());

				case "m" :
				case "membership" :
				{
					var rp = Program.Rdc<MembersResponse>(new MembersRequest());
	
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
