using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Web3;
using UC.Net;

namespace UC.Sun.CLI
{
	/// <summary>
	/// Usage: author bid 
	///						by = ACCOUNT 
	///						[password = PASSWORD]
	///						name = AUTHOR
	///						amount = UNT
	///
	///		   author register 
	///						by = ACCOUNT 
	///						[password = PASSWORD]
	///						name = AUTHOR
	///						title = TITLE
	///						years = NUMBER
	///
	///		   author transfer
	///						from = ACCOUNT
	///						[password = PASSWORD]
	///						name = AUTHOR
	///						to = ACCOUNT
	///	
	///		   author overview
	///						name = AUTHOR
	/// </summary>
	public class AuthorCommand : Command
	{
		public const string Keyword = "author";

		public AuthorCommand(Settings settings, Log log, Func<Core> core, Xon args) : base(settings, log, core, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "bid" : 
					return Core.Enqueue(new AuthorBid(	GetPrivate("by", "password"), 
														GetString("name"),
														Coin.ParseDecimal(GetString("amount"))), 
														GetAwaitStage(), 
														Workflow);
		   		case "register" : 
					return Core.Enqueue(new AuthorRegistration(	GetPrivate("by", "password"), 
																GetString("name"),
																GetString("title"),
																byte.Parse(GetString("years"))),
																GetAwaitStage(),
																Workflow);
		   		case "transfer" : 
					return Core.Enqueue(new AuthorTransfer(	GetPrivate("from", "password"), 
															GetString("name"),
															AccountAddress.Parse(GetString("to"))),
															GetAwaitStage(), 
															Workflow);

		   		case "overview" :
				{
					var i = Core.Connect(Role.Base, null, Workflow).GetAuthorInfo(GetString("name"));

					Workflow.Log?.Report(this, "Author", $"'{GetString("name")}' :");

					Dump(i.Entry.ToXon(new XonTextValueSerializator()));
										
					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
