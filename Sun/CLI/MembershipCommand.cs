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
using Uccs.Net;

namespace Uccs.Sun.CLI
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

		public MembershipCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "declare" : 
					return new CandidacyDeclaration(Money.ParseDecimal(GetString("bail")));
				
				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
