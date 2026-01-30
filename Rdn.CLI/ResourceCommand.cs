using System.Reflection;

namespace Uccs.Rdn.CLI;

public class ResourceCommand : RdnCommand
{
	Ura First => Ura.Parse(Args[0].Name);

	Argument Data => new ("data", HEX, "A data to be associated with the resource", Flag.Optional);
	Argument Dependable => new ("dependable", null, "Turns a resource into dependable one. Once linked by any number of Dependency links, this resources can not be changed or deleted", Flag.Optional);

	public ResourceCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";

		a.Description = "Creates a resource entity in the distributed database";
		a.Arguments =	[
							new (null, RA, "Address of a resource to create", Flag.First),
							Data,
							Dependable,
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								Transacted = () =>	{
														//var	r = Rdc(new ResourceRequest(First)).Resource;
												
														Api(new LocalResourceUpdateApc {Address = First,
																						Data = GetData()});
													};

								return new ResourceCreation(First, GetData(), Has(Dependable.Name));
							};
		return a;
	}

	public CommandAction Destroy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";

		a.Description = "Destroys existing resource and all its associated links";
		a.Arguments =	[
							new (null, RA, "Address of a resource to delete", Flag.First),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var r = Ppc(new ResourcePpc(First)).Resource;

								return new ResourceDeletion {Resource = r.Id};
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "u";
		a.Description = "Updates a resource entity properties in the distributed database";
		a.Arguments =	[
							new (null, RA, "Address of a resource to update", Flag.First),
							Data,
							Dependable,
							new ("recursive", null, "Update all descendants", Flag.Optional),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var	r = Ppc(new ResourcePpc(First)).Resource;

								Transacted = () =>	{
														Api(new LocalResourceUpdateApc {Address = First,
																						Data = GetData()});
													};

								var o =	new ResourceUpdation(r.Id);

								if(Has(Data.Name))			o.Change(GetData());
								if(Has(Dependable.Name))	o.MakeDependable();
								if(Has("recursive"))		o.MakeRecursive();

								return o;
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Gets resource entity information from the MCV database";
		a.Arguments =	[
							new (null, RA, "Address of a resource to get information about", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Ppc(new ResourcePpc(First)).Resource;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}

	public CommandAction Local()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Description = "Gets information about locally available releases of a specified resource";
		a.Arguments =	[
							new (null, RA, "Address of a resource to get information about", Flag.First)
						];

		a.Execute = () =>	{
								var r = Api<LocalResource>(new LocalResourceApc {Address = First});
				
								if(r != null)
								{
									Flow.Log.Dump(	r.Datas, 
													["Type", "Data", "Length"], 
													[i => i.Type, i => i.Value.ToHex(32), i => i.Value.Length]);

									return r;
								}
								else
									throw new Exception("Resource not found");
							};
		return a;
	}

	public CommandAction Local_Search()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "ls";
		a.Description = "Search local resources using a specified query";
		a.Arguments =	[
							new (null, RA, $"A text to look for in resource addresses (includes domain name)", Flag.First)
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var r = Api<IEnumerable<LocalResource>>(new LocalResourcesSearchApc {Query = Args.Any() ? Args[0].Name : null});
				
								Flow.Log.Dump(	r, 
												["Address", "Releases", "Type", "Data", "Length"], 
												[i =>	i.Address.Domain + '/' + i.Address.Resource,
														i => i.Datas.Count,
														i => i.Last.Type,
														i => i.Last.Value.ToHex(32),
														i => i.Last.Value.Length]);
								return r;
							};
		return a;
	}

	public CommandAction Download()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "d";

		a.Description = "Downloads the latest release of a specified resource";
		a.Arguments =	[
							new (null, RA, "Address of a resource the latest release to download of", Flag.First),
							new ("localpath", DIRPATH,	"Destination path on the local system to download the release to", Flag.Optional),
							new ("nowait", null, "Wait download to finish", Flag.Optional)
						];

		a.Execute = () =>	{
								var r = Api<Resource>(new ResourceDownloadApc{Identifier = new(First), LocalPath = GetString("localpath", null)});

								if(!Has("nowait"))
								{
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
								else
								{
									return r;
								}
							};
		return a;
	}

	public CommandAction Download_Cancelation()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "dx";
		a.Description = "Cancels current downloading of specified release";
		a.Arguments =	[
							new (null, RZA,  "Address of a release to cancel downloading of", Flag.First),
						];

		a.Execute = () =>	{
								Api(new CancelResourceDownloadApc {Release = Urr.Parse(Args[0].Name)});

								return null;
							};
		return a;
	}
}
