using System.Reflection;

namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class LinkCommand : RdnCommand
{
	public LinkCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Creates a link from one resource to another";
		a.Arguments =	[
							new ("from", RA, "The address of a source resource. Transaction user must be owner of resource domain."),
							new ("to", RA, "The address of a destination resource"),
							new ("type", LT, "The type of link to create", ArgumentFlag.Optional),
							DomainCommand.Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var s = Ppc(new ResourceByAddressPpc(GetResourceAddress(a[0].Name))).Resource;
								var d = Ppc(new ResourceByAddressPpc(GetResourceAddress(a[1].Name))).Resource;

								return new ResourceLinkCreation(s.Id, d.Id, GetEnum("type", ResourceLinkType.None));
							};
		return a;
	}

	public CommandAction Delete_X()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Destroys existing link";
		a.Arguments =	[
							new ("from", RA, "The address of a source resource. Transaction user must be owner of resource domain."),
							new ("index", INT, "The index of link to delete"),
							DomainCommand.Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var s = Ppc(new ResourceByAddressPpc(GetResourceAddress(a[0].Name))).Resource;

								return new ResourceLinkDeletion(s.Id, GetInt(a[1].Name));
							};
		return a;
	}

	public CommandAction ListOutbounds_LO()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Lists outbound links of the specified resource";
		a.Arguments =	[
							AddressOrId(RA, "resource to list outbound links from")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var r = Ppc(new ResourceByIdPpc(ResourceId));
				
								Flow.Log.Dump(	r.Resource.Outbounds.Select((o, i) => new {I = i, L = o, R = Ppc(new ResourceByIdPpc(o.Destination))}),
												["#",		"Type",			"Id",					"Address"],
												[i => i.I,	i => i.L.Type,	i => i.L.Destination,	i => i.R.Address]);

								return r;
							};
		return a;
	}

	public CommandAction ListInbounds_LI()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Lists inbound links of the specified resource";
		a.Arguments =	[
							AddressOrId(RA, "resource to list inbound links from")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var r = Ppc(new ResourceByIdPpc(ResourceId));
																	
								Flow.Log.Dump(	r.Resource.Inbounds.Select(i => new {L = i, R = Ppc(new ResourceByIdPpc(i))}),
												["Id",		"Address"],
												[i => i.L,	i => i.R.Address]);

								return r;
							};
		return a;
	}
}
