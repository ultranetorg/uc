using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Web3;

namespace UC.Net.Node.CLI
{
	/// <summary>
	/// Usage: membership declare
	///						candidate = ACCOUNT 
	///						[password = PASSWORD] 
	///						bail = UNT 
	///						ip = IP
	///							
	///	Example: membership declare candidate=0x0007f34bc43d41cf3ec2e6f684c7b9b131b04b41 bail=1000 ip=192.168.1.107
	/// </summary>
	public class MembershipCommand : Command
	{
		public const string Keyword = "membership";

		public MembershipCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "declare" : 
					return Core.Enqueue(new CandidacyDeclaration(	GetPrivate("candidate", "password"), 
																	Coin.ParseDecimal(GetString("bail")), 
																	IPAddress.Parse(GetString("ip"))),
																	GetAwaitStage(), 
																	Workflow);

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
