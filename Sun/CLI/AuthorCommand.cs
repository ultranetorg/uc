using System;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
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

		public AuthorCommand(Zone zone, Settings settings, Workflow workflow, Net.Sun sun, Xon args) : base(zone, settings, workflow, sun, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "bid" : 
					return new AuthorBid(	GetString("name"),
											GetStringOrEmpty("tld"),
											Coin.ParseDecimal(GetString("amount")));
		   		case "register" : 
					return new AuthorRegistration(	GetString("name"),
													GetString("title"),
													byte.Parse(GetString("years")));
		   		case "transfer" : 
					return new AuthorTransfer(	GetString("name"),
												AccountAddress.Parse(GetString("to")));
		   		case "info" :
				{
					try
					{
						var rp = Sun.Call(i => i.GetAuthorInfo(GetString("name")), Workflow);
	
						//Workflow.Log?.Report(this, "Author", $"'{GetString("name")}' :");
	
						Dump(rp.Author);
						
						return rp.Author;
					}
					catch(RdcEntityException ex)
					{
						Workflow.Log?.Report(this, ex.Message);
					}

					return null;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
