namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class ResourceCommand : RdnCommand
{
	public const string Keyword = "resource";

	Ura First => Ura.Parse(Args[0].Name);

	public ResourceCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help
							{ 
								Title = "CREATE",
								Description = "Creates a resource entity in the distributed database",
								Syntax = "resource c|create URA [flags] [data]",

								Arguments =
								[
									new ("<first>", "Address of a resource to create"),
									new ("data", "A data associated with the resource"),
									new ("seal", "If set, resource data cannot be changed anymore")
								],

								Examples =
								[
									new (null, "resource c company/application data=0105BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												Transacted = () =>	{
																		//var	r = Rdc(new ResourceRequest(First)).Resource;
												
																		Api(new LocalResourceUpdateApc {Address = First,
																										Data = GetData()});
																	};

												return new ResourceCreation(First, GetData(), Has("seal"));
											}
						},

						new ()
						{
							Names = ["x", "destroy"],

							Help = new Help
							{ 
								Title = "DESTROY",
								Description = "Destroys existing resource and all its associated links",
								Syntax = "resource x|destroy URA",

								Arguments =
								[
									new ("<first>", "Address of a resource to delete")
								],

								Examples =
								[
									new (null, "resource x company/application")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var r = Rdc(new ResourceRequest(First)).Resource;

												return new ResourceDeletion {Id = r.Id};
											}
						},

						new ()
						{
							Names = ["u", "update"],

							Help = new Help
							{ 
								Title = "UPDATE",
								Description = "Updates a resource entity properties in the distributed database",
								Syntax = "resource u|update URA [flags] [data] [recursive]",

								Arguments =
								[
									new ("<first>", "Address of a resource to update"),
									new ("data", "A data associated with the resource"),
									new ("seal", "If set, resource data cannot be changed anymore"),
									new ("recursive", "Update all descendants")
								],

								Examples =
								[
									new (null, "resource u company/application data=Package{address=urrh:BCE1C336874FBEBE40D2510EC035D0251FE855399EAD76E22BD18E2EBC6E37}")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var	r = Rdc(new ResourceRequest(First)).Resource;

												Transacted = () =>	{
																		Api(new LocalResourceUpdateApc {Address = First,
																										Data = GetData()});
																	};

												var o =	new ResourceUpdation(r.Id);

												if(Has("data"))			o.Change(GetData());
												if(Has("seal"))			o.Seal();
												if(Has("recursive"))	o.MakeRecursive();

												return o;
											}
						},

						new ()
						{
							Names = ["e", "entity"],

							Help = new Help
							{ 
								Title = "Entity",
								Description = "Gets resource entity information from the MCV database",
								Syntax = "resource e|entity URA",

								Arguments =
								[
									new ("<first>", "Address of a resource to get information about")
								],

								Examples =
								[
									new (null, "resource e company/application")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);

												var	r = Rdc(new ResourceRequest(First)).Resource;
				
												Dump(r);

												return r;
											}
						},

						new ()
						{
							Names = ["l", "local"],

							Help = new Help
							{ 
								Title = "LOCAL",
								Description = "Gets information about locally available releases of a specified resource",
								Syntax = "resource l|local URA",

								Arguments =
								[
									new ("<first>", "Address of a resource to get information about")
								],

								Examples =
								[
									new (null, "resource l company/application")
								]
							},

							Execute = () =>	{
												var r = Api<LocalResource>(new LocalResourceApc {Address = First});
				
												if(r != null)
												{
													Dump(	r.Datas, 
															["Type", "Data", "Length"], 
															[i => i.Type, i => i.Value.ToHex(32), i => i.Value.Length]);

													return r;
												}
												else
													throw new Exception("Resource not found");
											}
						},

						new ()
						{
							Names = ["ls", "localsearch"],

							Help = new Help
							{ 
								Title = "LOCAL SEARCH",
								Description = "Search local resources using a specified query",
								Syntax = "resource ls|localsearch query",

								Arguments =
								[
									new ("query", "A text to look for in resource addresses (includes domain name)")
								],

								Examples =
								[
									new (null, "resource ls appli")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);

												var r = Api<IEnumerable<LocalResource>>(new LocalResourcesSearchApc {Query = Args.Any() ? Args[0].Name : null});
				
												Dump(	r, 
														["Address", "Releases", "Type", "Data", "Length"], 
														[i => i.Address.Domain + '/' + i.Address.Resource,
														 i => i.Datas.Count,
														 i => i.Last.Type,
														 i => i.Last.Value.ToHex(32),
														 i => i.Last.Value.Length]);
												return r;
											}
						},

						new ()
						{
							Names = ["d", "download"],

							Help = new Help
							{
								Title = "DOWNLOAD",
								Description = "Downloads the latest release of a specified resource",
								Syntax = "resource d|download URA [localpath=PATH]",

								Arguments =
								[
									new ("<first>",	  "Address of a resource the latest release to download of"),
									new ("localpath", "Destination path on the local system to download the release to")
								],

								Examples =
								[
									new (null, "resource d company/application")
								]
							},

							Execute = () =>	{
												var r = Api<Resource>(new ResourceDownloadApc{Identifier = new(First), LocalPath = GetString("localpath", null)});

												while(Flow.Active)
												{
													var p = Api<ResourceActivityProgress>(new LocalReleaseActivityProgressApc {Release = r.Data.Parse<Urr>()});

													if(p is null)
														break;

													Report(p.ToString());

													Thread.Sleep(500);
												}

												return null;
											}
						},
					];
	}

}
