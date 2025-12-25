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
// 									new (FirstArg, "Address of a root domain to bid on"), // "<first>" placeholder replaced by actual usage context
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
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "a";
		a.Description = "Obtain ownership of a domain name for a specified period";
		a.Arguments =	[
							new (null, RDA, "Address of a root domain to be acquired", Flag.First),
							new ("years", YEARS, "Integer number of years in [1..10] range"),
							ByArgument("Address of account that owns or is going to register the domain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								///if(Domain.IsChild(First))
								///	throw new SyntaxException("Only root domains name are allowed");

								return new DomainRegistration{	Address	= First,
																Years	= byte.Parse(GetString("years"))};
							};

		return a;
	}

	public CommandAction Migrate()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "m";
		a.Description = "Request web domain migration";
		a.Arguments =	[
							new (null, RDA, "Ultranet address of a root domain to migrate", Flag.First),
							new ("wtld", TLD, "Web top-level domain (com, org, net, info, biz)"),
							ByArgument("Address of account for which TXT record must be created in DNS net of specified web domain as a proof of ownership")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new DomainMigration(First, GetString("wtld"));
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "r";

		a.Description = "Extend domain ownership for a specified period. It's allowed only during the last year of current period.";
		a.Arguments =	[
							new (null, DA, "Address of a domain to be renewed", Flag.First),
							new ("years", YEARS, "Integer number of years in [1..10] range"),
							ByArgument("Address of account that owns the domain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;

								return new DomainRenewal() {Id		= d.Id,
															Years	= byte.Parse(GetString("years"))};
							};

		return a;
	}

	public CommandAction Create_Subdomain()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "cs";
		a.Description = "Create a subdomain";
		a.Arguments =	[
							new (null, SDA, "Subdomain address to create", Flag.First),
							new ("policy", DCP, "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain"),
							new ("years", YEARS, "Number of years in [1..10] range"),
							new ("for", NAME, "Name of the account that will own the subdomain"),
							ByArgument("Address of account that owns the parent domain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var f = Ppc(new UserPpc(GetString(a.Arguments[3].Name)));

								return	new DomainRegistration
										{
											Address	= First,
											Policy	= GetEnum(a.Arguments[1].Name, DomainChildPolicy.FullOwnership),
											Years	= byte.Parse(GetString(a.Arguments[2].Name)),
											Owner	= f.User.Id
										};
							};
		return a;
	}

	public CommandAction Update_Policy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "up";
		a.Description = "Changes current policy of subdomain";
		a.Arguments =	[
							new (null, SDA, "Address of a domain to change policy for", Flag.First),
							new ("policy", DCP, "FullOwnership - the owner of parent domain can later revoke/change ownership of subdomain, FullFreedom - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy"),
							ByArgument("Address of account that owns a subdomain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;

								return new DomainPolicyUpdation {Id		= d.Id,
																 Policy	= GetEnum(a.Arguments[1].Name, DomainChildPolicy.FullOwnership)};
							};
		return a;
	}

	public CommandAction Transfer()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "t";
		a.Description = "Changes an owner of domain";
		a.Arguments =	[
							new (null, DA, "Address of a domain to transfer", Flag.First),
							new ("to", NAME, "A name of a new owner"),
							ByArgument("Address of account of the current owner")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;
								var to = Ppc(new UserPpc(GetString(a.Arguments[1].Name))).User;

								return new DomainTransfer  {Id		= d.Id,
															Owner	= to.Id};
							};

		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get domain entity information from MCV database";
		a.Arguments =	[
							new (null, DA, "Address of a domain to get information about", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new DomainPpc(First));

								Flow.Log.Dump(rp.Domain);
					
								return rp.Domain;
							};

		return a;
	}
}
