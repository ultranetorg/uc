using System;
using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	public class AccountCommand : Command
	{
		public const string Keyword = "account";
		AccountAddress		First => AccountAddress.Parse(Args[0].Name);

		public AccountCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["e", "entity"],
								Help = new Help	{ 
													Title = "Entity",
													Description = "Get account entity information from Ultranet distributed database",
													Syntax = "account e|entity UAA",

													Arguments = [new ("<first>", "Address of an account to get information about")],

													Examples = [new (null, "account e 0x0000fffb3f90771533b1739480987cee9f08d754")]
												},

								Execute = () =>	{
													Flow.CancelAfter(RdcQueryTimeout);

													var i = Rdc(new AccountRequest(First));
														
													Dump(i.Account);

													return i.Account;
												}
							}
						];		
		}
	}
}
