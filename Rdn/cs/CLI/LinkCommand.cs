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
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a link from one resource to another",
						Syntax = $"{Keyword} {a.NamesSyntax} from={RA} to={RA}",

						Arguments =	[
										new ("from", "Address of a source resource. Transaction signer must be owner of this resource."),
										new ("to", "Address of a destination resource")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} from={RA.Example[0]} to={RA.Example[1]}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var s = Ppc(new ResourceRequest(GetResourceAddress("from"))).Resource;
								var d = Ppc(new ResourceRequest(GetResourceAddress("to"))).Resource;

								return new ResourceLinkCreation(s.Id, d.Id);
							};
		return a;
	}

	public CommandAction Destroy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Help = new() {Description = "Destroys existing link",
						Syntax = $"{Keyword} {a.NamesSyntax} from={RA} to={RA}",

						Arguments =	[
										new ("from", "Address of a source resource. Transaction signer must be owner of this resource."),
										new ("to", "Address of a destination resource")
									],

						Examples =
									[
										new (null, $"{Keyword} {a.Name} from={RA.Example[0]} to={RA.Example[1]}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var s = Ppc(new ResourceRequest(GetResourceAddress("from"))).Resource;
								var d = Ppc(new ResourceRequest(GetResourceAddress("to"))).Resource;

								return new ResourceLinkDeletion(s.Id, d.Id);
							};
		return a;
	}

	public CommandAction List_Outbounds()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "lo";

		a.Help = new() {Description = "Lists outbound links of a specified resource",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA}",

						Arguments =	[
										new ("<first>", "Address of a resource which outbound links are be listed of")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]};

		a.Execute = () =>	{
							Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

							var r = Ppc(new ResourceRequest(Ura.Parse(Args[0].Name)));
				
							Flow.Log.Dump(	r.Resource.Outbounds.Select(i => new {L = i, R = Ppc(new ResourceRequest(i.Destination)).Resource}),
									["#", "Flags", "Destination", "Destination Data"],
									[i => i.L.Destination, i => i.L.Flags, i => i.R.Address, i => i.R.Data?.ToString()]);

							return r;
						};
		return a;
	}

	public CommandAction List_Inbounds()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "li";

		a.Help = new() {Description = "Lists inbound links of a specified resource",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA}",

						Arguments =	[
										new ("<first>", "Address of a resource which inbound links are be listed of")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]
						};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var r = Ppc(new ResourceRequest(Ura.Parse(Args[0].Name)));
																	
								Flow.Log.Dump(	r.Resource.Inbounds.Select(i => new {L = i, R = Ppc(new ResourceRequest(i)).Resource}),
												["#", "Source", "Source Data"],
												[i => i.L, i => i.R.Address, i => i.R.Data?.ToString()]);

								return r;
							};
		return a;
	}
}
