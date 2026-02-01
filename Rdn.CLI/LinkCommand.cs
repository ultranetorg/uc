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

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Creates a link from one resource to another";
		a.Arguments =	[
							new ("from", RA, "Address of a source resource. Transaction signer must be owner of this resource."),
							new ("to", RA, "Address of a destination resource"),
							new ("type", LT, "The type of link to create", Flag.Optional),
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var s = Ppc(new ResourcePpc(GetResourceAddress(a[0].Name))).Resource;
								var d = Ppc(new ResourcePpc(GetResourceAddress(a[1].Name))).Resource;

								return new ResourceLinkCreation(s.Id, d.Id, GetEnum("type", ResourceLinkType.None));
							};
		return a;
	}

	public CommandAction Delete()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Description = "Destroys existing link";
		a.Arguments =	[
							new ("from", RA, "Address of a source resource. Transaction signer must be owner of this resource."),
							new ("index", INT, "An index of link to delete")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var s = Ppc(new ResourcePpc(GetResourceAddress(a[0].Name))).Resource;

								return new ResourceLinkDeletion(s.Id, GetInt(a[1].Name));
							};
		return a;
	}

	public CommandAction List_Outbounds()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "lo";

		a.Description = "Lists outbound links of a specified resource";
		a.Arguments =	[
							new (null, RA, "Address of a resource which outbound links are be listed of", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var r = Ppc(new ResourcePpc(Ura.Parse(Args[0].Name)));
				
								Flow.Log.Dump(	r.Resource.Outbounds.Select((o, i) => new {I = i, L = o, R = Ppc(new ResourcePpc(o.Destination)).Resource}),
												["#",		"Type",			"To Id",				"To Address"],
												[i => i.I,	i => i.L.Type,	i => i.L.Destination,	i => i.R.Address]);

								return r;
							};
		return a;
	}

	public CommandAction List_Inbounds()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "li";

		a.Description = "Lists inbound links of a specified resource";
		a.Arguments =	[
							new (null, RA, "Address of a resource which inbound links are be listed of", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var r = Ppc(new ResourcePpc(Ura.Parse(Args[0].Name)));
																	
								Flow.Log.Dump(	r.Resource.Inbounds.Select(i => new {L = i, R = Ppc(new ResourcePpc(i)).Resource}),
												["#", "Source", "Source Data"],
												[i => i.L, i => i.R.Address, i => i.R.Data?.ToString()]);

								return r;
							};
		return a;
	}
}
