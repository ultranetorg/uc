using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DomainCommand : RdnCommand
{
	public static readonly Argument Eligible = ByArgument("Name of the user eligible to change the domain");
	public static readonly Argument Years = new ("years", YEARS, "Number of years in [1..10] range");
	public static readonly Argument Policy = new ("policy", DCP, $"{OwnershipPolicy.FullOwnership} - the owner of the parent domain can later revoke/change ownership of subdomain, {OwnershipPolicy.FullFreedom} - the owner of the parent domain can NOT later revoke/change ownership of the subdomain or change policy");

	new AutoId Id(string nameaddress)
	{
		if(Has(IdKeyword))
			return GetAutoId(IdKeyword);
		else if(Has(nameaddress))
			return Ppc(new DomainByNamePpc(GetString(nameaddress))).Domain.Id;
		else
			throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");
	}

	Domain GetDomain()
	{
		if(Has(IdKeyword))
		{	
			var r = Ppc(new DomainByIdPpc(GetAutoId(IdKeyword)));
			return r.Domain;
		}
		else if(Has(NameKeyword))
		{	
			var r = Ppc(new DomainByNamePpc(GetString(NameKeyword)));
			return r.Domain;
		}
		else
			throw new SyntaxException("Neither 'id' nor 'name' arguments provided");
	}

	public DomainCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public DomainCommand()
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string @for = nameof(@for);

		a.Description = "Create a domain or subdomain";
		a.Arguments =	[
							new (NameKeyword, DN, "domain or subdomain name to assign"),
							Policy,
							Years,
							new (@for, NAME, "Name of the user that will own the subdomain"),
							ByArgument("Domain owner username")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								if(DomainName.IsRoot(Name))
								{
									return	new DomainCreation
											{
												Name	= Name,
												Years	= byte.Parse(GetString(Years.Name))
											};
								} 
								else
								{
									var f = Ppc(new UserByNamePpc(GetString(@for)));
	
									return	new DomainCreation
											{
												Name	= Name,
												Policy	= GetEnum(Policy.Name, OwnershipPolicy.FullOwnership),
												Years	= byte.Parse(GetString(Years.Name)),
												Owner	= f.User.Id
											};
								}
							};
		return a;
	}

	public CommandAction Name_N()
	{
		const string newname = nameof(newname);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Assign a new name for the domain";
		a.Arguments	  =	[
							NameOrIdOf("domain to rename", DN),
							new (newname, NAME, "New domain name"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return	new DomainRenaming
										{
											Domain = Id(NameKeyword), 
											NewName = GetString(newname)
										};
							};
		return a;
	}


	public CommandAction Renewal_R()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Extend domain ownership for the specified period. It's allowed only during the last year of current period.";
		a.Arguments =	[
							NameOrIdOf("root domain to renew", DN),
							Years,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new DomainRenewal()
										{
											Id		= Id(NameKeyword),
											Years	= byte.Parse(GetString(Years.Name))
										};
							};

		return a;
	}

	public CommandAction UpdatePolicy_UP()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Changes current policy of subdomain";
		a.Arguments =	[
							NameOrIdOf("domain to change policy for", DN),
							Policy,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainPolicyUpdation {Id		= Id(NameKeyword),
																 Policy	= GetEnum(Policy.Name, OwnershipPolicy.FullOwnership)};
							};
		return a;
	}

	public CommandAction Security_S()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string owner = nameof(owner);

		a.Description = "Manages security for the specified domain";
		a.Arguments =	[
							NameOrIdOf("domain to manage security of", DN),
							new (owner, NAME, "Name of the new owner"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var to = Ppc(new UserByNamePpc(GetString(owner))).User;

								return new DomainTransfer
										{
											Id		= Id(NameKeyword),
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
							NameOrIdOf("domain to get information about", DN),
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var d = GetDomain();

								Flow.Log.Dump(d);
					
								return d;
							};

		return a;
	}
}
