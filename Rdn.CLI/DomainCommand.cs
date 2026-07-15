using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DomainCommand : RdnCommand
{
	public static readonly Argument Eligible = ByArgument("Name of the user eligible to change Domain entity");
	public static readonly Argument Years = new ("years", YEARS, "Integer number of years in [1..10] range");
	public static readonly Argument Policy = new ("policy", DCP, $"{DomainChildPolicy.FullOwnership} - the owner of the parent domain can later revoke/change ownership of subdomain, {DomainChildPolicy.FullFreedom} - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy");

	new AutoId Id(string nameaddress)
	{
		if(Has(IdKeyword))
			return GetAutoId(IdKeyword);
		else if(Has(nameaddress))
			return Ppc(new DomainPpc(GetString(nameaddress))).Domain.Id;
		else
			throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");
	}

	public DomainCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Migrate_M()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Requests domain name acquisition by verifying web domain ownership";
		a.Arguments =	[
							NameArgument(RDN, "root domain name to migrate"),
							new ("wtld", TLD, "Web top-level domain (com, org, net, info, biz)"),
							ByArgument("Name of the user for which TXT record must be created in DNS zone of specified web domain as a proof of ownership")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainMigration(Name, GetString("wtld"));
							};
		return a;
	}

	public CommandAction Renew_R()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Extend domain ownership for the specified period. It's allowed only during the last year of current period.";
		a.Arguments =	[
							NameOrId(RDN, "root domain to be renewed"),
							Years,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainRenewal() {Id		= Id(NameKeyword),
															Years	= byte.Parse(GetString(Years.Name))};
							};

		return a;
	}


	public CommandAction Acquire_A()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string @for = nameof(@for);

		a.Description = "Register a domain or subdomain";
		a.Arguments =	[
							AddressArgument(DA, "domain or subdomain to create"),
							Policy,
							Years,
							new (@for, NAME, "Name of the account that will own the subdomain"),
							ByArgument("Name of the user that is going to take  or give a domain")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								if(Domain.IsRoot(Address))
								{
									return	new DomainRegistration
											{
												Address	= Address,
												Years	= byte.Parse(GetString(Years.Name))
											};
								} 
								else
								{
									var f = Ppc(new UserPpc(GetString(@for)));
	
									return	new DomainRegistration
											{
												Address	= Address,
												Policy	= GetEnum(Policy.Name, DomainChildPolicy.FullOwnership),
												Years	= byte.Parse(GetString(Years.Name)),
												Owner	= f.User.Id
											};
								}
							};
		return a;
	}

	public CommandAction UpdatePolicy_UP()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Changes current policy of subdomain";
		a.Arguments =	[
							AddressOrId(RDN, "domain to change policy for"),
							Policy,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainPolicyUpdation {Id		= Id(AddressKeyword),
																 Policy	= GetEnum(Policy.Name, DomainChildPolicy.FullOwnership)};
							};
		return a;
	}

	public CommandAction Security_S()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string owner = nameof(owner);

		a.Description = "Manages security for the specified domain";
		a.Arguments =	[
							AddressOrId(DA, "domain to manage security of"),
							new (owner, NAME, "Name of the new owner"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var to = Ppc(new UserPpc(GetString(owner))).User;

								return new DomainTransfer
										{
											Id		= Id(AddressKeyword),
											Owner	= to.Id
										};
							};

		return a;
	}

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get domain entity information from MCV database";
		a.Arguments =	[
							AddressOrId(DA, "domain to get information about"),
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new DomainPpc(Id(AddressKeyword)));

								Flow.Log.Dump(rp.Domain);
					
								return rp.Domain;
							};

		return a;
	}
}
