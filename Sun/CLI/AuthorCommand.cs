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

		public AuthorCommand(Program program, Xon args) : base(program, args)
		{
		}

		public override object Execute()
		{
			if(!Args.Nodes.Any())
				throw new SyntaxException("Operation is not specified");

			switch(Args.Nodes.First().Name)
			{
		   		case "b" : 
		   		case "bid" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorBid(	Args.Nodes[1].Name,
											GetString("tld", ""),
											Money.ParseDecimal(GetString("amount")));
				}

		   		case "r" : 
		   		case "register" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorRegistration(	Args.Nodes[1].Name,
													byte.Parse(GetString("years")));
				}

		   		case "t" : 
		   		case "transfer" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new AuthorTransfer(	Args.Nodes[1].Name,
												AccountAddress.Parse(GetString("to")));
				}

		   		case "e" :
		   		case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);
					
					var rp = Rdc<AuthorResponse>(new AuthorRequest {Name = Args.Nodes[1].Name});
	
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
