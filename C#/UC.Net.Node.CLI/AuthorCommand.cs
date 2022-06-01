using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Web3;

namespace UC.Net.Node.CLI
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
					return Send(() => Client.Enqueue(new AuthorBid(	GetPrivate("by", "password"), 
																	GetString("name"),
																	Coin.ParseDecimal(GetString("amount")))));
		   		case "register" : 
					return Send(() => Client.Enqueue(new AuthorRegistration(GetPrivate("by", "password"), 
																			GetString("name"),
																			GetString("title"),
																			byte.Parse(GetString("years")))));
		   		case "transfer" : 
					return Send(() => Client.Enqueue(new AuthorTransfer(GetPrivate("from", "password"), 
																		GetString("name"),
																		Account.Parse(GetString("to")))));
		   		case "overview" :
				{
					var i = Client.Api.Send(new AuthorInfoCall{ Name =  GetString("name"), Confirmed = Args.Has("confirmed") });

					Log.Report(this, "Author", $"'{GetString("name")}' :");

					Dump(i);
										
					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
