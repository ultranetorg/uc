using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class AccountCommand : Command
	{
		public const string Keyword = "account";

		public AccountCommand(Program program, Xon args) : base(program, args)
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

					var i = Rdc<AccountResponse>(new AccountRequest {Account = AccountAddress.Parse(Args.Nodes[1].Name)});
	
					Dump(i.Account);

					return i.Account;
				}

				default:
					throw new SyntaxException("Unknown operation");
			}
		}
	}
}
