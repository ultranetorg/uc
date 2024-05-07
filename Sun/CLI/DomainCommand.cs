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

		DomainIdentifier Identifier
		{
			get
			{
				if(Has("a"))
					return new DomainIdentifier(GetString("a"));

				if(Has("id"))
					return new DomainIdentifier(EntityId.Parse(GetString("id")));

				throw new SyntaxException("address or id required");
			}
		}

		public DomainCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["b", "bid"],

								Help = new Help
								{ 
									Title = "BID",
									Description = "Domain names that starts from alphanumeric symbol are distributed via auction. Once the first bid is confirmed, an auction is considered to have started and every next bid extends it by 1 month and lasts for 1 year at least.",
									Syntax = "domain b|bid a=URDA amount=AMOUNT by=ACCOUNT",

									Arguments = 
									[
										new ("a", "Address of a root domain to bid on"), // "<first>" placeholder replaced by actual usage context
										new ("amount", "Amount of bid in UNT"),
										new ("by", "Address of account that makes a bid and will own the domain if wins")
									],

									Examples =
									[
										new (null, "domain b a=companyinc amount=1.000 by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													return new DomainBid(GetString("a"), Money.ParseDecimal(GetString("amount")));
												}
							},


							new ()
							{
								Names = ["a", "acquire"],

								Help = new Help()
								{
									Title = "ACQUIRE",
									Description = "Obtain ownership of a domain name for a specified period",
									Syntax = "domain a|acquire a=URDA years=YEARS by=ACCOUNT",

									Arguments =
									[
										new ("a", "Address of a root domain to be acquired"),
										new ("years", "Integer number of years in [1..10] range"),
										new ("by", "Address of account that owns or is going to register the domain")
									],

									Examples =
									[
										new (null, "domain a a=companyinc years=5 by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													return new DomainRegistation{	Address	= GetString("a"),
																					Years	= byte.Parse(GetString("years"))};
												}
							},

							new ()
							{
								Names = ["m", "migrate"],

								Help = new Help()
								{
									Title = "MIGRATE",
									Description = "Request web domain migration",
									Syntax = "domain m|migrate a=URDA wtld=WTLD [rank] by=ACCOUNT",

									Arguments =
									[
										new ("a", "Ultranet address of a root domain to migrate"),
										new ("wtld", "Web top-level domain (com, org, net)"),
										new ("rank", "Request position verification in Google search results"),
										new ("by", "Address of account for which TXT record must be created in DNS zone of specified web domain as a proof of ownership")
									],

									Examples =
									[
										new (null, "domain m a=apple wtld=com checkrank by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													return new DomainMigration(GetString("a"), GetString("tld"), Has("checkrank"));
												}
							},

							new ()
							{
								Names = ["r", "renew"],

								Help = new Help()
								{
									Title = "RENEW",
									Description = "Extend domain ownership for a specified period",
									Syntax = "domain r|renew a=UDA|id=DID years=YEARS by=ACCOUNT",

									Arguments =
									[
										new ("a/id", "Address/id of a domain to be renewed"),
										new ("years", "Integer number of years in [1..10] range"),
										new ("by", "Address of account that owns the domain")
									],

									Examples =
									[
										new (null, "domain r a=companyinc years=5 by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													var d = Rdc(new DomainRequest(Identifier)).Domain;

													return new DomainUpdation  {Action	= DomainAction.Renew,
																				Id		= d.Id,
																				Years	= byte.Parse(GetString("years"))};
												}
							},

							new ()
							{
								Names = ["cs", "createsubdomain"],

								Help = new Help()
								{
									Title = "CREATE SUBDOMAIN",
									Description = "Create a subdomain",
									Syntax = "domain cs|createsubdomain a=USDA policy=POLICY years=YEARS for=ACCOUNT by=ACCOUNT",

									Arguments =
									[
										new ("a", "Subdomain address to create"),
										new ("policy", "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain"),
										new ("years", "Number of years in [1..10] range"),
										new ("for", "Address of account that will own the subdomain"),
										new ("by", "Address of account that owns the parent domain")
									],

									Examples =
									[
										new (null, "domain cs a=division.companyinc years=5 policy=FullOwnership for=0x2222222222222222222222222222222222222222 by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													return new DomainRegistation{	Address	= GetString("a"),
																					Years	= byte.Parse(GetString("years")),
																					Policy	= GetEnum("policy", DomainChildPolicy.FullOwnership),
																					Owner	= GetAccountAddress("for")};
												}
							},

							new ()
							{
								Names = ["up", "updatepolicy"],

								Help = new Help()
								{
									Title = "UPDATE POLICY",
									Description = "Changes current policy of subdomain",
									Syntax = "domain up|updatepolicy a=USDA|id=DID policy=POLICY by=ACCOUNT",

									Arguments =
									[
										new ("<first>", "Address/id of a domain to change policy for"),
										new ("policy", "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy"),
										new ("by", "Address of account that owns a subdomain")
									],

									Examples =
									[
										new (null, "domain up a=division.companyinc policy=FullOwnership by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													var d = Rdc(new DomainRequest(Identifier)).Domain;

													return new DomainUpdation  {Action	= DomainAction.ChangePolicy,
																				Id		= d.Id,
																				Policy	= GetEnum("policy", DomainChildPolicy.FullOwnership)};
												}
							},

							new ()
							{
								Names = ["t", "transfer"],

								Help = new Help()
								{
									Title = "TRANSFER",
									Description = "Changes an owner of domain",
									Syntax = "domain t|transfer a=UDA|id=DID to=ACCOUNT by=ACCOUNT",

									Arguments =
									[
										new ("<first>", "Address/id of a domain to transfer"),
										new ("to", "Address of account of a new owner"),
										new ("by", "Address of account of the current owner")
									],

									Examples =
									[
										new (null, "domain transfer a=companyinc to=0x2222222222222222222222222222222222222222 by=0x0000fffb3f90771533b1739480987cee9f08d754")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													var d = Rdc(new DomainRequest(Identifier)).Domain;

													return new DomainUpdation  {Action	= DomainAction.Transfer,
																				Id		= d.Id,
																				Owner	= GetAccountAddress("to", false)};
												}
							},

							new ()
							{
								Names = ["e", "entity"],

								Help = new Help()
								{
									Title = "Entity",
									Description = "Get domain entity information from MCV database",
									Syntax = "domain e|entity a=UDA|id=DID",

									Arguments =
									[
										new ("<first>", "Address/id of a domain to get information about")
									],

									Examples =
									[
										new (null, "domain e a=companyinc")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcQueryTimeout);
					
													var rp = Rdc(new DomainRequest(Identifier));
	
													Dump(rp.Domain);
						
													return rp.Domain;
												}
							},

						];	
		}
	}
}
