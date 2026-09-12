using System.Reflection;

namespace Uccs.Rdn.CLI;

public class DomainNameCommand : RdnCommand
{
	public static readonly Argument Eligible = ByArgument("Name of the user eligible to access domain name");

	public DomainNameCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public DomainNameCommand()
	{
	}

	public CommandAction Migration_M()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Initiates priority name migration by verifying web domain ownership";
		a.Arguments =	[
							new (NameKeyword, DN, $"Domain name to migrate"),
							new ("wtld", TLD, $"Web top-level domain ({string.Join(", ", DomainName.PriorityTlds)})"),
							ByArgument("Name of the user for which TXT record must be created in DNS zone of specified web domain as a proof of ownership")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new DomainNameMigration(Name, GetString("wtld"));
							};
		return a;
	}

	public CommandAction Acquisition_A()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string @for = nameof(@for);

		a.Description = "Acquires a domain name";
		a.Arguments =	[
							new (NameKeyword, DN, $"Domain name to acquire"),
							DomainCommand.Years,
							ByArgument("Name of the user that is going to take the name")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new DomainNameAcquisition
										{
											Name	= Name,
											Years	= byte.Parse(GetString(DomainCommand.Years.Name))
										};
							};
		return a;
	}

	public CommandAction Renewal_R()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Extends domain name ownership for the specified period";
		a.Arguments =	[
							new (NameKeyword, DN, $"Domain name to renew"),
							DomainCommand.Years,
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new DomainNameRenewal
										{
											Name	= Name,
											Years	= byte.Parse(GetString(DomainCommand.Years.Name))
										};
							};

		return a;
	}

	public CommandAction Security_S()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string owner = nameof(owner);

		a.Description = "Manages security for the specified domain name";
		a.Arguments =	[
							NameOrIdOf("domain name to manage security of", DN),
							new (owner, NAME, "New owner username"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var to = Ppc(new UserByNamePpc(GetString(owner))).User;

								return new DomainNameTransfer
										{
											Name	= Name,
											Owner	= to.Id
										};
							};

		return a;
	}
}
