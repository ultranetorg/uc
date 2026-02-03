using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageCommand : NexusCommand
{
	Ura	Package => Ura.Parse(Args[0].Name);

	public readonly ArgumentType PA 	= new ("PA", "Package resource address", [@"company/application/windows/1.2.3"]);
	public readonly ArgumentType APR 	= new ("APR", "Realization address", [@"company/application/windows"]);
	public readonly ArgumentType APRV 	= new ("APRV", "Release address", [@"company/application/windows/1.2.3", @"company/application/windows/4.5.6"]);

	public PackageCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Builds and deploys a package to a node filebase for distribution via RDN";
		a.Arguments =	[
							new (null,				APRV,		"Resource address of package to create", Flag.First),
							new ("previous",		APRV,		"Address of previous release", Flag.Optional),
							new ("source",			PATH,		"A list of paths to files separated by comma", Flag.Multi),
							new ("dependencies",	FILEPATH,	"A path to version manifest file where complete dependencies are defined", Flag.Optional),
							new ("cr",				null,		"Create resource", Flag.Optional),
							new ("cdl",				null,		"Create dependency links ", Flag.Optional),
						];

		a.Examples =	() =>	[
								new (null, @$"{Keyword} {a.Name} {APRV.Example} previous={APRV.Example1} source={FILEPATH.Example} source{FILEPATH.Example1} dependencies={DIRPATH}\1.2.3.{PackageManifest.Extension}")
							];

		a.Execute = () =>	{
								var dp = GetString("dependencies", null);

								var r = Api<LocalReleaseApe>(new PackageBuildApc   
															 {
																Resource		 = Ura.Parse(Args[0].Name), 
																Sources			 = Args.Where(i => i.Name == "source").Select(i => i.Get<string>()), 
																DependenciesPath = dp,
																Previous		 = GetResourceAddress("previous", false),
																AddressCreator	 =	new()
																					{
																						Type = GetEnum("addresstype", UrrScheme.Urrh),
																						Owner = GetAccountAddress("owner", false),
																						Resource = Ura.Parse(Args[0].Name)
																					}
															 });
								Flow.Log.Dump(r);

								List<Operation> ops = [];

								var rdn = new RdnApiClient(Net.Api.ForNode(Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone), Cli.Nexus.Settings.Api.LocalIP, false), null);

								if(Has(a.Arguments[4].Name))
								{
									ops.Add(new ResourceCreation(Ura.Parse(Args[0].Name), rdn.Call<LocalResource>(new LocalResourceApc {Address = Ura.Parse(Args[0].Name)}, Flow).Last, false));
								}

								if(Has(a.Arguments[5].Name))
								{
									var pm = PackageManifest.Load(dp);

									var id = Has(a.Arguments[4].Name) ? AutoId.LastCreated : rdn.Call<ResourcePpr>(new PpcApc(new ResourcePpc(Ura.Parse(Args[0].Name))), Flow).Resource.Id;
	
									ops.AddRange(pm.CompleteDependencies.Select(i => new ResourceLinkCreation(id, 
																											  rdn.Call<ResourcePpr>(new PpcApc(new ResourcePpc(i.Address)), Flow).Resource.Id,
																											  ResourceLinkType.Dependency)));
								}
								
								if(ops.Any())
									Transact(rdn, ops, GetString(McvCommand.ByArg), McvCommand.GetActionOnResult(Args));

								return ops;
							};
		return a;
	}

	public CommandAction Local()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Description = "Gets information about local copy of a specified package";
		a.Arguments =	[
							new (null, PA, "Address of local package to get information about", Flag.First)
						];

		a.Execute = () =>	{
							var r = Api<PackageInfo>(new LocalPackageApc {Address = Package});
				
							Flow.Log.Dump(r);

							return null;
						};
		return a;
	}

	public CommandAction Download()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "d";
		a.Description = "Downloads a specified package by its address";
		a.Arguments =	[
							new (null, PA, "Address of a package to download", Flag.First)
						];

		a.Execute = () =>	{
								Api(new PackageDownloadApc {Package = Package});

								try
								{
									do
									{
										var d = Api<ResourceActivityProgress>(new PackageActivityProgressApc {Package = Package});
						
										if(d is null)
										{	
											if(!Api<PackageInfo>(new LocalPackageApc {Address = Package}).Available)
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

	public CommandAction Deploy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "dp";
		a.Description = "If needed, downloads specified package and its dependencies recursively and deploys its content to the 'Packages' directory";
		a.Arguments =	[
							new (null, PA, "Address of a package to install", Flag.First),
							new ("destination", DIRPATH, "Packages destination path", Flag.Optional)
						];

		a.Execute = () =>	{
								Api(new PackageDeployApc{Address = ApvAddress.Parse(Args[0].Name),
														 DeploymentPath = GetString("destination", null)});

								try
								{
									do
									{
										var d = Api<ResourceActivityProgress>(new PackageActivityProgressApc {Package = Package});
						
										if(d is null)
										{	
											if(!Api<PackageInfo>(new LocalPackageApc {Address = Package}).Available)
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
