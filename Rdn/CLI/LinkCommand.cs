using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Rdn.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class LinkCommand : RdnCommand
	{
		public const string Keyword = "link";

		public LinkCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["c", "create"],

								Help = new Help
								{
									Title = "CREATE",
									Description = "Creates a link from one resource to another",
									Syntax = "link c|create from=URA to=URA",

									Arguments =
									[
										new ("from", "Address of a source resource. Transaction signer must be owner of this resource."),
										new ("to", "Address of a destination resource")
									],

									Examples =
									[
										new (null, "link c from=company/application/win32/1.3.4 to=company/application")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													var s = Rdc(new ResourceRequest(GetResourceAddress("from"))).Resource;
													var d = Rdc(new ResourceRequest(GetResourceAddress("to"))).Resource;

													return new ResourceLinkCreation(s.Id, d.Id);
												}
							},

							new ()
							{
								Names = ["d", "destroy"],

								Help = new Help
								{ 
									Title = "DESTROY",
									Description = "Destroys existing link",
									Syntax = "link x|destroy from=URA to=URA",

									Arguments =
									[
										new ("from", "Address of a source resource. Transaction signer must be owner of this resource."),
										new ("to", "Address of a destination resource")
									],

									Examples =
									[
										new (null, "link x from=company/application/win32/1.2.3 to=company/application")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

													var s = Rdc(new ResourceRequest(GetResourceAddress("from"))).Resource;
													var d = Rdc(new ResourceRequest(GetResourceAddress("to"))).Resource;

													return new ResourceLinkDeletion(s.Id, d.Id);
												}
							},

							new ()
							{
								Names = ["lo", "listoutbounds"],

								Help = new Help
								{ 
									Title = "LIST OUTBOUNDS",
									Description = "Lists outbound links of a specified resource",
									Syntax = "link lo|listoutbounds URA",

									Arguments =
									[
										new ("<first>", "Address of a resource which outbound links are be listed of")
									],

									Examples =
									[
										new (null, "link lo company/application")
									]							
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcQueryTimeout);

													var r = Rdc(new ResourceRequest(Ura.Parse(Args[0].Name)));
					
													Dump(r.Resource.Outbounds.Select(i => new {L = i, R = Rdc(new ResourceRequest(i.Destination)).Resource}),
														 ["#", "Flags", "Destination", "Destination Data"],
														 [i => i.L.Destination, i => i.L.Flags, i => i.R.Address, i => i.R.Data?.Interpretation]);

													return r;
												}
							},

							new ()
							{
								Names = ["li", "listinbounds"],

								Help = new Help
								{ 
									Title = "LIST INBOUNDS",
									Description = "Lists inbound links of a specified resource",
									Syntax = "link li|listinbounds URA",

									Arguments =
									[
										new ("<first>", "Address of a resource which inbound links are be listed of")
									],

									Examples =
									[
										new (null, "link li company/application")
									]								
								},

								Execute = () =>	{
													Flow.CancelAfter(program.Settings.RdcQueryTimeout);

													var r = Rdc(new ResourceRequest(Ura.Parse(Args[0].Name)));
																		
													Dump(r.Resource.Inbounds.Select(i => new {L = i, R = Rdc(new ResourceRequest(i)).Resource}),
														 ["#", "Source", "Source Data"],
														 [i => i.L, i => i.R.Address, i => i.R.Data?.Interpretation]);

													return r;
												}
							},
						];
		}
	}
}
