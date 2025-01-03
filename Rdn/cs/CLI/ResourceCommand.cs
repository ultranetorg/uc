using System.Reflection;

namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class ResourceCommand : RdnCommand
{
	Ura First => Ura.Parse(Args[0].Name);

	public ResourceCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";

		a.Help = new() {Description = "Creates a resource entity in the distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA} [flags] [data={HEX}]",

						Arguments =	[
										new ("<first>", "Address of a resource to create"),
										new ("data", "A data associated with the resource"),
										new ("seal", "If set, resource data cannot be changed anymore")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example} data={HEX.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								Transacted = () =>	{
														//var	r = Rdc(new ResourceRequest(First)).Resource;
												
														Api(new LocalResourceUpdateApc {Address = First,
																						Data = GetData()});
													};

								return new ResourceCreation(First, GetData(), Has("seal"));
							};
		return a;
	}

	public CommandAction Destroy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";

		a.Help = new() {Description = "Destroys existing resource and all its associated links",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA}",

						Arguments =	[
										new ("<first>", "Address of a resource to delete")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} company/application")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var r = Rdc(new ResourceRequest(First)).Resource;

								return new ResourceDeletion {Resource = r.Id};
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";
		a.Help = new() {Description = "Updates a resource entity properties in the distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA} [flags] [data] [recursive]",

						Arguments =
									[
										new ("<first>", "Address of a resource to update"),
										new ("data", "A data associated with the resource"),
										new ("seal", "If set, resource data cannot be changed anymore"),
										new ("recursive", "Update all descendants")
									],

						Examples =
									[
										new (null, $"{Keyword} {a.Name} {RA.Example} data=Package{{address={RZA.Example}}}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

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
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Gets resource entity information from the MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA}",

						Arguments =	[
										new ("<first>", "Address of a resource to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Rdc(new ResourceRequest(First)).Resource;
				
								Dump(r);

								return r;
							};
		return a;
	}

	public CommandAction Local()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Gets information about locally available releases of a specified resource",
						Syntax = $"{Keyword} {a.NamesSyntax} {RA}",

						Arguments =	[
										new ("<first>", "Address of a resource to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]};

		a.Execute = () =>	{
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
							};
		return a;
	}

	public CommandAction Local_Search()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "ls";
		a.Help = new() {Description = "Search local resources using a specified query",
						Syntax = $"{Keyword} {a.NamesSyntax} <query>",

						Arguments =
									[
										new ("<query>", $"A {TEXT} to look for in resource addresses (includes domain name)")
									],

						Examples =
									[
										new (null, $"{Keyword} {a.Name} appli")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var r = Api<IEnumerable<LocalResource>>(new LocalResourcesSearchApc {Query = Args.Any() ? Args[0].Name : null});
				
								Dump(	r, 
										["Address", "Releases", "Type", "Data", "Length"], 
										[i => i.Address.Domain + '/' + i.Address.Resource,
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
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "d";

		a.Help = new() {Description = "Downloads the latest release of a specified resource",
						Syntax = $"r{Keyword} {a.NamesSyntax} {RA} [localpath={DIRPATH}]",

						Arguments =	[
										new ("<first>",		"Address of a resource the latest release to download of"),
										new ("localpath",	"Destination path on the local system to download the release to"),
										new ("nowait",		"Wait downlonad to finish")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]};

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
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "dx";

		a.Help = new() {Description = "Cancels current downloading of specified release",
						Syntax = $"{Keyword} {a.NamesSyntax} {RZA}",

						Arguments =	[
										new ("<first>",	  "Address of a release to cancel downloading of"),
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RA.Example}")
									]};

		a.Execute = () =>	{
								Api(new CancelResourceDownloadApc {Release = Urr.Parse(Args[0].Name)});

								return null;
							};
		return a;
	}
}
