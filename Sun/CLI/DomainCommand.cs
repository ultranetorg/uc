using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: domain bid 
	///						by = ACCOUNT 
	///						[password = PASSWORD]
	///						name = DOMAIN
	///						amount = UNT
	///
	///		   domain register 
	///						by = ACCOUNT 
	///						[password = PASSWORD]
	///						name = DOMAIN
	///						title = TITLE
	///						years = NUMBER
	///
	///		   domain transfer
	///						from = ACCOUNT
	///						[password = PASSWORD]
	///						name = DOMAIN
	///						to = ACCOUNT
	///	
	///		   domain overview
	///						name = DOMAIN
	/// </summary>
	public class DomainCommand : Command
	{
		public const string Keyword = "domain";

		public DomainCommand(Program program, List<Xon> args) : base(program, args)
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

					return new DomainBid(	Args[1].Name,
											Money.ParseDecimal(GetString("amount")));
				}
		   		case "m" : 
		   		case "migrate" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new DomainMigration(Args[1].Name, GetString("tld"), Has("checkrank"));
				}

		   		case "r" : 
		   		case "register" : 
				{
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new DomainRegistration  {Address		= Args[1].Name,
													Years		= byte.Parse(GetString("years")),
													ChildPolicy = GetEnum("childpolicy", DomainChildPolicy.None),
													Owner		= GetAccountAddress("owner", false)};
				}

		   		case "t" : 
		   		case "transfer" : 
				{	
					Workflow.CancelAfter(RdcTransactingTimeout);

					return new DomainTransfer(	Args[1].Name,
												AccountAddress.Parse(GetString("to")));
				}

		   		case "e" :
		   		case "entity" :
				{
					Workflow.CancelAfter(RdcQueryTimeout);
					
					var rp = Rdc(new DomainRequest {Name = Args[1].Name});
	
					//Workflow.Log?.Report(this, "Domain", $"'{GetString("name")}' :");
	
					rp.Domain.Id = rp.EntityId;
					Dump(rp.Domain);
						
					return rp.Domain;
				}

				default:
					throw new SyntaxException("Unknown operation");;
			}
		}
	}
}
