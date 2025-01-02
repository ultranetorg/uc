using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DomainCommand : RdnCommand
{
	string First => Args[0].Name;

	public DomainCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
// 						new ()
// 						{
// 							Names = ["b", "bid"],
// 
// 							Help = new Help
// 							{ 
// 								Title = "BID",
// 								Description = "Domain names that starts from alphanumeric symbol are distributed via auction. Once the first bid is confirmed, an auction is considered to have started and every next bid extends it by 1 month and lasts for 1 year at least.",
// 								Syntax = $"domain b|bid {RDA} amount=UNT signer={UAA}",
// 
// 								Arguments = 
// 								[
// 									new ("<first>", "Address of a root domain to bid on"), // "<first>" placeholder replaced by actual usage context
// 									new ("amount", "Amount of bid in UNT"),
// 									new (SignerArg, "Address of account that makes a bid and will own the domain if wins")
// 								],
// 
// 								Examples =
// 								[
// 									new (null, "domain b companyinc amount=1.000 signer=0x0000fffb3f90771533b1739480987cee9f08d754")
// 								]
// 							},
// 
// 							Execute = () =>	{
// 												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);
// 
// 												return new DomainBid(First, long.Parse(GetString("amount")));
// 											}
// 						},


	}

	public CommandAction Acquire()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "a";
		a.Help = new() {Description = "Obtain ownership of a domain name for a specified period",
						Syntax = $"{Keyword} {a.NamesSyntax} {RDA} years={YEARS} signer={AA}",

						Arguments =	[
										new ("<first>", "Address of a root domain to be acquired"),
										new ("years", "Integer number of years in [1..10] range"),
										new (SignerArg, "Address of account that owns or is going to register the domain")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RDA.Example} years=5 {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
							Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

							return new DomainRegistration{	Address	= First,
															Years	= byte.Parse(GetString("years"))};
						};

		return a;
	}

	public CommandAction Migrate()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "m";
		a.Help = new() {Description = "Request web domain migration",
						Syntax = $"{Keyword} {a.NamesSyntax} {RDA} wtld={TLD} [rank] {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Ultranet address of a root domain to migrate"),
										new ("wtld", "Web top-level domain (com, org, net)"),
										new ("checkrank", "Request position verification in Google search results"),
										new (SignerArg, "Address of account for which TXT record must be created in DNS net of specified web domain as a proof of ownership")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RDA.Example} wtld={TLD} checkrank {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new DomainMigration(First, GetString("wtld"), Has("checkrank"));
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "r";

		a.Help = new() {Description = "Extend domain ownership for a specified period. It's allowed only during the last year of current period.",
						Syntax = $"{Keyword} {a.NamesSyntax} {DA} years={YEARS} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Address of a domain to be renewed"),
										new ("years", "Integer number of years in [1..10] range"),
										new (SignerArg, "Address of account that owns the domain")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {DA.Example} years=5 {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Rdc(new DomainRequest(First)).Domain;

								return new DomainUpdation  {Action	= DomainAction.Renew,
															Id		= d.Id,
															Years	= byte.Parse(GetString("years"))};
							};

		return a;
	}

	public CommandAction Create_Subdomain()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "cs";
		a.Help = new() {Description = "Create a subdomain",
						Syntax = $"{Keyword} {a.NamesSyntax} {SDA} policy=POLICY years={YEARS} for={AA} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Subdomain address to create"),
										new ("policy", "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain"),
										new ("years", "Number of years in [1..10] range"),
										new ("for", "Address of account that will own the subdomain"),
										new (SignerArg, "Address of account that owns the parent domain")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {SDA.Example} years=5 policy=FullOwnership for={AA.Examples[1]} {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new DomainRegistration{	Address	= First,
																Years	= byte.Parse(GetString("years")),
																Policy	= GetEnum("policy", DomainChildPolicy.FullOwnership),
																Owner	= GetAccountAddress("for")};
							};
		return a;
	}

	public CommandAction Update_Policy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "up";
		a.Help = new(){	Description = "Changes current policy of subdomain",
						Syntax = $"{Keyword} {a.NamesSyntax} {SDA} policy=POLICY signer={AA}",

						Arguments =	[
										new ("<first>", "Address of a domain to change policy for"),
										new ("policy", "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy"),
										new (SignerArg, "Address of account that owns a subdomain")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {SDA.Example} policy=FullOwnership {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Rdc(new DomainRequest(First)).Domain;

								return new DomainUpdation  {Action	= DomainAction.ChangePolicy,
															Id		= d.Id,
															Policy	= GetEnum("policy", DomainChildPolicy.FullOwnership)};
							};
		return a;
	}

	public CommandAction Transfer()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "t";
		a.Help = new() {Description = "Changes an owner of domain",
						Syntax = $"domain t|transfer {DA} to={AA} signer={AA}",

						Arguments =	[
										new ("<first>", "Address of a domain to transfer"),
										new ("to", "Address of account of a new owner"),
										new (SignerArg, "Address of account of the current owner")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {DA.Example} to={AA.Example[1]} {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Rdc(new DomainRequest(First)).Domain;

								return new DomainUpdation  {Action	= DomainAction.Transfer,
															Id		= d.Id,
															Owner	= GetAccountAddress("to", false)};
							};

		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get domain entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {DA}",

						Arguments =	[
										new ("<first>", "Address of a domain to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {DA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new DomainRequest(First));

								Dump(rp.Domain);
					
								return rp.Domain;
							};

		return a;
	}
}
