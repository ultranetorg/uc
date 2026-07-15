using System.Reflection;

namespace Uccs.Rdn.CLI;

public class ResourceCommand : RdnCommand
{
	Argument	Data		=> new ("data", HEX, "Data to be associated with the resource", ArgumentFlag.Optional);
	Argument	Dependable	=> new ("dependable", null, "Turns a resource into dependable one. Once linked by any number of Dependency links, this resources can not be changed or deleted", ArgumentFlag.Optional);

	(Resource resource, Ura address) GetResource()
	{
		if(Has(IdKeyword))
		{	
			var r = Ppc(new ResourceByIdPpc(GetAutoId(IdKeyword)));
			return (r.Resource, r.Address);
		}
		else if(Has(AddressKeyword))
		{	
			var r = Ppc(new ResourceByAddressPpc(Ura.Parse(GetString(AddressKeyword))));
			return (r.Resource, Ura.Parse(Address));
		}
		else
			throw new SyntaxException("Neither 'id' nor 'name' arguments provided");
	}

	public ResourceCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Creates a resource entity in the distributed database";
		a.Arguments =	[
							AddressArgument(RA, "resource to create"),
							Data,
							Dependable,
							DomainCommand.Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								Transacted = () =>	{
														//var	r = Rdc(new ResourceRequest(First)).Resource;
												
														Api(new LocalResourceUpdateApc {Address = Ura.Parse(Address),
																						Data = GetData()});
													};

								return new ResourceCreation(Ura.Parse(Address), GetData(), Has(Dependable.Name));
							};
		return a;
	}

	public CommandAction Delete_X()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Destroys existing resource and all its associated links";
		a.Arguments =	[
							AddressOrId(RA, "resource to delete"),
							DomainCommand.Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ResourceDeletion {Resource = ResourceId};
							};
		return a;
	}

	public CommandAction Update_U()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string recursive = nameof(recursive);

		a.Description = "Updates a resource properties";
		a.Arguments =	[
							AddressOrId(RA, "resource to update"),
							Data,
							Dependable,
							new (recursive, null, "Update all descendants", ArgumentFlag.Optional),
							DomainCommand.Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var	r = GetResource();

								Transacted = () =>	{
														Api(new LocalResourceUpdateApc {Address = r.address,
																						Data = GetData()});
													};

								var o =	new ResourceUpdation(r.resource.Id);

								if(Has(Data.Name))			o.Change(GetData());
								if(Has(Dependable.Name))	o.MakeDependable();
								if(Has(recursive))			o.MakeRecursive();

								return o;
							};
		return a;
	}

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets resource entity information from the MCV database";
		a.Arguments =	[
							AddressOrId(RA, "resource to get information about"),
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								Flow.Log.Dump(GetResource().resource);

								return null;
							};
		return a;
	}

	public CommandAction Local_L()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about locally available releases of the specified resource";
		a.Arguments =	[
							AddressOrId(RA, "resource local copy to get information about")
						];

		a.Execute = () =>	{
								var r = Api<LocalResource>(new LocalResourceApc {Address = Ura.Parse(Address)});
				
								if(r != null)
								{
									Flow.Log.Dump(r);

									return r;
								}
								else
									throw new Exception("Resource not found");
							};
		return a;
	}

	public CommandAction LocalSearch_LS()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string query = nameof(query);

		a.Description = "Search local resources using the specified query";
		a.Arguments =	[
							new (query, STRING, $"Full or partial address of the resource, including domain name")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var r = Api<IEnumerable<LocalResource>>(new LocalResourcesSearchApc {Query = GetString(query)});
				
								Flow.Log.Dump(	r, 
												["Address", "Type", "Data", "Length"], 
												[i =>	i.Address.Domain + '/' + i.Address.Resource,
														i => i.Data.Type,
														i => i.Data.Value.ToHex(32),
														i => i.Data.Value.Length]);
								return r;
							};
		return a;
	}

	public CommandAction Download_D()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string to = nameof(to);
		const string wait = nameof(wait);

		a.Description = "Downloads the latest release of the specified resource";
		a.Arguments =	[
							AddressOrId(RA, "resource the latest release to download of"),
							new (to,		DIRPATH,	"Destination path on the local system to download the release to", ArgumentFlag.Optional),
							new (wait,		BOOL,		"Wait or not download to finish", ArgumentFlag.Optional, "yes")
						];

		a.Execute = () =>	{
								var r = GetResource().resource;

								Api(new ResourceDownloadApc{Id = r.Id, To = GetString(to, null)});

								if(GetBool(wait, true))
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

	public CommandAction DownloadCancelation_DX()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Cancels current download process of specified release";
		a.Arguments =	[
							new (AddressKeyword, RZA,  "Address of the release to cancel downloading of"),
						];

		a.Execute = () =>	{
								Api(new CancelResourceDownloadApc {Release = Urr.Parse(GetString(AddressKeyword))});

								return null;
							};
		return a;
	}
}
