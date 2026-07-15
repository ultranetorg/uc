using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;

namespace Uccs.Nexus.CLI;

public class PackageCommand : NexusCommand
{
	public static readonly		ArgumentType PA = new ("PA", "Package resource address", [@"/company/application/winx64/1.2.3"]);

	Ura address => Ura.Parse(Address);

	public PackageCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string previous = nameof(previous);
		const string source = nameof(source);
		const string dependencies = nameof(dependencies);
		const string cr = nameof(cr);
		const string cdl = nameof(cdl);

		a.Description = "Builds and deploys a package to a node file base for distribution via RDN";
		a.Arguments =	[
							new (AddressKeyword,PA,			"Resource address of the package being created"),
							new (previous,		PA,			"Resource address of the previous release", ArgumentFlag.Optional),
							new (source,		PATH,		"File or directory paths of the content to be packaged", ArgumentFlag.Multi),
							new (dependencies,	FILEPATH,	"Path to the version manifest file where complete dependencies are defined", ArgumentFlag.Optional),
							new (cr,			null,		"Creates corresponding resource in RDN database", ArgumentFlag.Optional),
							new (cdl,			null,		"Creates dependency links in RDN database", ArgumentFlag.Optional),
						];

		a.Examples =	() =>	[
									new (null, @$"{Keyword} {a.Name} {PA.Example} {previous}={AprvAddress.Parse(PA.Example).APR}/1.0.0 {source}={FILEPATH.Example} {source}={DIRPATH.Example} {dependencies}={DIRPATH.Example1}\{AprvAddress.Parse(PA.Example).Version}.{PackageManifest.Extension}")
								];

		a.Execute = () =>	{

								var dp = GetString(dependencies, null);

								var r = Api<LocalReleaseApe>(new PackageBuildApc   
															 {
																Resource		 = address, 
																Sources			 = Args.Where(i => i.Name == source).Select(i => i.Get<string>()), 
																DependenciesPath = dp,
																Previous		 = GetResourceAddress(previous, false),
																AddressCreator	 =	new()
																					{
																						Type = UrrScheme.Rrrh,
																						///Owner = GetAccountAddress("owner", null),
																						Resource = address
																					}
															 });
								Flow.Log.Dump(r);

								List<Operation> ops = [];

								var rdn = new RdnApiClient(Net.Api.ForNode(Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone), Cli.Nexus.Settings.Api.LocalIP, false), null);

								if(Has(cr))
								{
									ops.Add(new ResourceCreation(address, rdn.Call<LocalResource>(new LocalResourceApc {Address = address}, Flow).Data, false));
								}

								if(Has(cdl))
								{
									var pm = PackageManifest.Load(dp);

									var id = Has(cr) ? AutoId.LastCreated : rdn.Ppc(new ResourceByAddressPpc(address), Flow).Resource.Id;
	
									ops.AddRange(pm.CompleteDependencies.Select(i => new ResourceLinkCreation(id, 
																											  rdn.Ppc(new ResourceByAddressPpc(i.Address), Flow).Resource.Id,
																											  ResourceLinkType.Dependency)));
								}
								
								if(ops.Any())
									Transact(rdn, ops, GetString(ByKeyword), GetLong(BoostKeyword, 0), McvCommand.GetActionOnResult(Args));

								return ops;
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
								var r = Api<PackageInfo>(new LocalPackageApc {Address = address});
				
								Flow.Log.Dump(r);

								return null;
							};
		return a;
	}

	public CommandAction Download_D()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Downloads a package from the specified address";
		a.Arguments =	[
							new (AddressKeyword, PA, "Resource address of the package to download", ArgumentFlag.First)
						];

		a.Execute = () =>	{
								Api(new StartPackageDownloadApc {Package = address});

								try
								{
									do
									{
										var d = Api<PackageActivityProgress>(new PackageActivityProgressApc {Package = address});
						
										if(d is null)
										{	
											if(!Api<PackageInfo>(new LocalPackageApc {Address = address}).Available)
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
							new (AddressKeyword, PA, "Resource address of the package to install", ArgumentFlag.First),
							new (to, DIRPATH, "Destination path for all package contents", ArgumentFlag.Optional)
						];

		a.Execute = () =>	{
								Api(new PackageDeployApc
									{
										Address = address,
										To = GetString(to, null)}
									);

								try
								{
									do
									{
										var d = Api<PackageActivityProgress>(new PackageActivityProgressApc {Package = address});
						
										if(d is null)
										{	
											if(!Api<PackageInfo>(new LocalPackageApc {Address = address}).Available)
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
