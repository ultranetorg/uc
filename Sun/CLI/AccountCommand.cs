using System;
using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class AccountCommand : Command
	{
		public const string Keyword = "account";

		public AccountCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
				case "e" :
				case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);

					var i = Rdc(new AccountRequest {Account = AccountAddress.Parse(Args[1].Name)});
	
					Dump(i.Account);

					return i.Account;
				}

				default:
					throw new SyntaxException("Unknown operation");
			}
		}
	}
}
