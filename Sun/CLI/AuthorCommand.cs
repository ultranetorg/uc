using System;
using System.Collections.Generic;
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

		public AuthorCommand(Program program, List<Xon> args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.First().Name)
			{
		   		case "b" : 
		   		case "bid" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorBid(	Args[1].Name,
											Money.ParseDecimal(GetString("amount")));
				}
		   		case "m" : 
		   		case "migrate" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorMigration(Args[1].Name, GetString("tld"), Has("checkrank"));
				}

		   		case "r" : 
		   		case "register" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorRegistration(	Args[1].Name,
													byte.Parse(GetString("years")));
				}

		   		case "t" : 
		   		case "transfer" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorTransfer(	Args[1].Name,
												AccountAddress.Parse(GetString("to")));
				}

		   		case "e" :
		   		case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);
					
					var rp = Rdc(new AuthorRequest {Name = Args[1].Name});
	
					//Workflow.Log?.Report(this, "Author", $"'{GetString("name")}' :");
	
					rp.Author.Id = rp.EntityId;
					Dump(rp.Author);
						
					return rp.Author;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
