using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DomainCommand : RdnCommand
{
	public static Argument Eligible => ByArgument("Name of the user eligible to change Domain entity");
	public static Argument Years => new ("years", YEARS, "Integer number of years in [1..10] range");
	public static Argument Policy = new ("policy", DCP, $"{DomainChildPolicy.FullOwnership} - the owner of the parent domain can later revoke/change ownership of subdomain, {DomainChildPolicy.FullFreedom} - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy");

	public DomainCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Migrate()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "m";
		a.Description = "Request web domain migration";
		a.Arguments =	[
							new (null, RDA, "Address of a root domain to migrate", Flag.First),
							new ("wtld", TLD, "Web top-level domain (com, org, net, info, biz)"),
							ByArgument("Name of the user for which TXT record must be created in DNS zone of specified web domain as a proof of ownership")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainMigration(First, GetString("wtld"));
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "r";

		a.Description = "Extend domain ownership for the specified period. It's allowed only during the last year of current period.";
		a.Arguments =	[
							new (null, DA, "Address of a domain to be renewed", Flag.First),
							Years,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;

								return new DomainRenewal() {Id		= d.Id,
															Years	= byte.Parse(GetString("years"))};
							};

		return a;
	}


	public CommandAction Acquire()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "a";
		a.Description = "Register a domain or subdomain";
		a.Arguments =	[
							new (null, SDA, "Subdomain address to create", Flag.First),
							Policy,
							Years,
							new ("for", NAME, "Name of the account that will own the subdomain"),
							ByArgument("Name of the user that is going to take  or give a domain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								if(Domain.IsRoot(First))
								{
									return	new DomainRegistration
											{
												Address	= First,
												Years	= byte.Parse(GetString("years"))
											};
								} 
								else
								{
									var f = Ppc(new UserPpc(GetString(a.Arguments[3].Name)));
	
									return	new DomainRegistration
											{
												Address	= First,
												Policy	= GetEnum(a.Arguments[1].Name, DomainChildPolicy.FullOwnership),
												Years	= byte.Parse(GetString(a.Arguments[2].Name)),
												Owner	= f.User.Id
											};
								}
							};
		return a;
	}

	public CommandAction Update_Policy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "up";
		a.Description = "Changes current policy of subdomain";
		a.Arguments =	[
							new (null, SDA, "An address of a subdomain to change policy for", Flag.First),
							Policy,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;

								return new DomainPolicyUpdation {Id		= d.Id,
																 Policy	= GetEnum(a.Arguments[1].Name, DomainChildPolicy.FullOwnership)};
							};
		return a;
	}

	public CommandAction Security()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "s";
		a.Description = "Manages security for the specified domain";
		a.Arguments =	[
							new (null, DA, "Address of a domain to transfer", Flag.First),
							new ("owner", NAME, "A name of a new owner"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var d = Ppc(new DomainPpc(First)).Domain;
								var to = Ppc(new UserPpc(GetString(a.Arguments[1].Name))).User;

								return new DomainTransfer
										{
											Id		= d.Id,
											Owner	= to.Id
										};
							};

		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get domain entity information from MCV database";
		a.Arguments =	[
							new (null, DA, "An address of a domain to get information about", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new DomainPpc(First));

								Flow.Log.Dump(rp.Domain);
					
								return rp.Domain;
							};

		return a;
	}
}
