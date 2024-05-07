using System.Collections.Generic;
using System.Linq;
using Uccs.Net;

namespace Uccs.Sun.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class LinkCommand : Command
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
									Syntax = "link c|create sa=URA|sid=RID da=URA|did=RID",

									Arguments =
									[
										new ("sa/sid", "Address/Id of a source resource. Transaction signer must be owner of this resource."),
										new ("da/did", "Address/Id of a destination resource")
									],

									Examples =
									[
										new (null, "link c sa=company/application/win32/1.2.3 da=company/application")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													var s = Rdc(new ResourceRequest(GetResourceIdentifier("sa", "sid"))).Resource;
													var d = Rdc(new ResourceRequest(GetResourceIdentifier("da", "did"))).Resource;

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
									Syntax = "link x|destroy source=RESOURCE_ADDRESS destination=RESOURCE_ADDRESS",

									Arguments =
									[
										new ("sa/sid", "Address/Id of a source resource of the link. Transaction signer must be owner of this resource."),
										new ("da/did", "Address/Id of a destination resource of the link")
									],

									Examples =
									[
										new (null, "link x sa=company/application/win32/1.2.3 da=company/application")
									]
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcTransactingTimeout);

													var s = Rdc(new ResourceRequest(GetResourceIdentifier("sa", "sid"))).Resource;
													var d = Rdc(new ResourceRequest(GetResourceIdentifier("da", "did"))).Resource;

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
									Syntax = "link lo|listoutbounds a=URA|id=RID",

									Arguments =
									[
										new ("a/id", "Address/Id of a resource which outbound links are be listed of")
									],

									Examples =
									[
										new (null, "link lo a=company/application")
									]							
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcQueryTimeout);

													var r = Rdc(new ResourceRequest(ResourceIdentifier));
					
													Dump(r.Resource.Outbounds.Select(i => new {L = i, R = Rdc(new ResourceRequest(i.Destination)).Resource}),
														 ["#", "Flags", "Destination", "Destination Data"],
														 [i => i.L.Destination, i => i.L.Flags, i => i.R.Address, i => i.R.Data.Interpretation]);

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
									Syntax = "link li|listinbounds a=URA|id=RID",

									Arguments =
									[
										new ("a/id", "Address/Id of a resource which inbound links are be listed of")
									],

									Examples =
									[
										new (null, "link li a=company/application")
									]								
								},

								Execute = () =>	{
													Flow.CancelAfter(RdcQueryTimeout);

													var r = Rdc(new ResourceRequest(ResourceIdentifier));
																		
													Dump(r.Resource.Inbounds.Select(i => new {L = i, R = Rdc(new ResourceRequest(i)).Resource}),
														 ["#", "Source", "Source Data"],
														 [i => i.L, i => i.R.Address, i => i.R.Data.Interpretation]);

													return r;
												}
							},
						];
		}
	}
}
