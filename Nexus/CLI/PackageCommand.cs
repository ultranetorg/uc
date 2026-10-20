using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Rdn.CLI;

namespace Uccs.Nexus.CLI;

public class PackageCommand : NexusCommand
{
	public static readonly		ArgumentType PA = new ("PA", "Package resource address", [@"/company/application/winx64/1.2.3"]);
	new Ura						Address => Ura.Parse(base.Address);
	RdnApiClient				Rapi;

	public PackageCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
		Rapi = new RdnApiClient(Net.Api.ForNode(Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone), Cli.Nexus.Settings.Api.LocalIP));
	}

	public PackageCommand()
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string source			= nameof(source);
		const string manifest		= nameof(manifest);
		const string instruction	= nameof(instruction);
		const string publish		= nameof(publish);
		const string link			= nameof(link);
		const string depandable		= nameof(depandable);

		a.Description = "Builds and deploys a package to a node file base for distribution via RDN";
		a.Arguments =	[
							new (AddressKeyword,	PA,				$"Package resource address"),
							new (source,			PATH,			"File or directory paths of the content to be packaged", ArgumentFlag.Multi),
							new (manifest,			FILEPATH,		"Path to the version manifest file where complete dependencies are defined", ArgumentFlag.Optional),
							new (instruction,		FILEPATH,		"Path to the instruction file", ArgumentFlag.Optional),
							new (publish,			null,			$"Creates corresponding resource in {Rdn.Rdn.Any.Title} database", ArgumentFlag.Optional),
							new (link,				null,			$"Creates dependency links in {Rdn.Rdn.Any.Title} database", ArgumentFlag.Optional),
							new (depandable,		null,			$"Marks created resource as dependable", ArgumentFlag.Optional),
						];

		a.Examples =	() =>	[
									new (null, @$"{Keyword} {a.Name} {source}={FILEPATH.Example} {source}={DIRPATH.Example} {manifest}={DIRPATH.Example1}\{AprvAddress.Parse(PA.Example).Version}.{PackageManifest.Extension} {instruction}={DIRPATH.Example1}\{AprvAddress.Parse(PA.Example).Version}.{PackageInstruction.Extension}")
								];

		a.Execute = () =>	{
								var mt = Has(manifest) ? File.ReadAllText(GetString(manifest)) : null;
								var it = Has(instruction) ? File.ReadAllText(GetString(instruction)) : null;

								PackageManifest m = null;

								if(mt != null)
								{
									m = PackageManifest.Parse(mt);
									//m.TranslateAddressToId(a => { 
									//								var r = Rapi.Ppc(new ResourceByAddressPpc(a), Flow).Resource;
									//
									//								Report($"Resource address resolved {a} -> {r.Id}");
									//
									//								return r.Id;
									//							});
									//mt = m.ToXon().ToString();
									//m = PackageManifest.Parse(mt);
								}

								AutoId getid(Ura addres) 
								{ 
									if(addres == null)
										return null;

									var r = Rapi.Ppc(new ResourceByAddressPpc(addres), Flow).Resource;
									
									Report($"Resource address resolved {addres} -> {r.Id}");
									
									return r.Id;
								}

								var p = Api(new PackageCreateApc   
											{
												Address			= Address,
												Sources			= Args.Where(i => i.Name == source).Select(i => i.Get<string>()), 
												Manifest		= mt,
												Instruction		= it,
												Previous		= getid(m?.Parents.FirstOrDefault()?.Address),
												AddressCreator	= new (UrnNamespace.Blake3)
											});
								
								Report($"Release created");
								Flow.Log.Dump(p);
								
								List<Operation> ops = [];

								if(Has(publish))
								{
									ops.Add(new ResourceCreation(Address, new ResourceData(Meaning.Package_Software_PackageManifest, p.Release.Address), Has(depandable)));
								}

								if(Has(link))
								{
									var id = Has(publish) ? AutoId.LastCreated 
														  : Rapi.Ppc(new ResourceByAddressPpc(Address), Flow).Resource.Id;
	
									if(m?.Parents.FirstOrDefault() != null)
										ops.Add(new ResourceLinkCreation(id, getid(m.Parents.First().Address), ResourceLinkType.Dependency));

									ops.AddRange(p.Manifest.Dependencies.Select(i => new ResourceLinkCreation(id, getid(i.Address), ResourceLinkType.Dependency)));
								}
								
								if(ops.Any())
								{	
									Transact(Rapi, ops, GetString(ByKeyword), GetLong(BoostKeyword, 0), McvCommand.GetActionOnResult(Args));

									var r = Rapi.Ppc(new ResourceByAddressPpc(Address), Flow).Resource;

									if(Has(publish))
									{
										Report($"Published under the resource with Id={r.Id}");

										Api(new PackageUpdateApc
											{
												Address = Address,
												Id = r.Id,
											});

										Report($"Resource Id assigned to package");

										Rapi.Send(	new ReleaseUpdateApc
													{
														Address = p.Release.Address,
														Id= r.Id,
													}, 
													Flow);

										Report($"Id assigned to release");
									}

									if(Has(link))
										foreach(var j in r.Outbounds)
											Report($"Dependency link created to {Rapi.Ppc(new ResourceByIdPpc(j.Destination), Flow).Address}");
								}

								return p;
							};
		return a;
	}

	public CommandAction Local_L()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about local copy of the specified package";
		a.Arguments =	[
							new (AddressKeyword, PA, "Resource address of the local package to get information about")
						];

		a.Execute = () =>	{
								var r = Api(new LocalPackageApc {Address = Address});
				
								Flow.Log.Dump(r);

								return null;
							};
		return a;
	}

	public CommandAction LocalSearch_LS()
	{
		const string query 	= nameof(query);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Searches for local packeges using the specified pattern";
		a.Arguments =	[
							new (query, STRING, "A text resource address contains")
						];

		a.Execute = () =>	{
								var r = Api(new LocalPackageSearchApc {Query = GetString(query)});
				
								Flow.Log.Dump(r, ["Address", "Id", "Urn"], [i => i.Address, i => i.Id, i => i.Release.Address]);

								return null;
							};
		return a;
	}

	public CommandAction Download_D()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Downloads a package from the specified address";
		a.Arguments =	[
							new (AddressKeyword, PA, "Resource address of the package to download")
						];

		a.Execute = () =>	{
								Api(new StartPackageDownloadApc {Id = Address});

								try
								{
									do
									{
										var d = Api(new PackageActivityProgressApc {Address = Address});
						
										if(d is null)
										{	
											if(!Api(new LocalPackageApc {Address = Address}).Available)
											{
												Flow.Log?.ReportError(this, "Failed");
											}

											break;
										}

										Report(d.ToString());

										Thread.Sleep(500);
									}
									while(Flow.Active);
								}
								catch(OperationCanceledException)
								{
								}

								return null;
							};
		return a;
	}

	public CommandAction Deploy_DP()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string to = nameof(to);

		a.Description = "If needed, downloads the specified package and its dependencies recursively and deploys its content to the default or specified directory";
		a.Arguments =	[
							new (AddressKeyword, PA, "Resource address of the package to install"),
							new (to, DIRPATH, "Destination path for all package contents", ArgumentFlag.Optional)
						];

		a.Execute = () =>	{
								Api(new PackageDeployApc
									{
										Address = Address,
										To = GetString(to, null)}
									);

								try
								{
									do
									{
										var d = Api(new PackageActivityProgressApc {Address = Address});
						
										if(d is null)
										{	
											if(!Api(new LocalPackageApc {Address = Address}).Available)
											{
												Flow.Log?.ReportError(this, "Failed");
											}

											break;
										}

										Report(d.ToString());

										Thread.Sleep(500);
									}
									while(Flow.Active);
								}
								catch(OperationCanceledException)
								{
								}
							
								return null;
							};
		return a;
	}
}
