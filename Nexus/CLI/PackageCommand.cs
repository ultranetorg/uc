using System.Reflection;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Rdn.CLI;

namespace Uccs.Nexus.CLI;

public class PackageCommand : NexusCommand
{
	public static readonly		ArgumentType PA = new ("PA", "Package resource address", [@"/company/application/winx64/1.2.3"]);

	Ura address => Ura.Parse(Address);

	RdnApiClient rdn;

	public PackageCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
		rdn =  new RdnApiClient(Net.Api.ForNode(Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone), Cli.Nexus.Settings.Api.LocalIP));
	}

	public PackageCommand()
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string previous		= nameof(previous);
		const string source			= nameof(source);
		const string manifest		= nameof(manifest);
		const string instruction	= nameof(instruction);
		const string cdl			= nameof(cdl);
		const string depandable		= nameof(depandable);

		a.Description = "Builds and deploys a package to a node file base for distribution via RDN";
		a.Arguments =	[
							new (source,			PATH,			"File or directory paths of the content to be packaged", ArgumentFlag.Multi),
							new (previous,			RdnCommand.RZA,	"Release address of parent package against which incremental package is build", ArgumentFlag.Optional),
							new (manifest,			FILEPATH,		"Path to the version manifest file where complete dependencies are defined", ArgumentFlag.Optional),
							new (instruction,		FILEPATH,		"Path to the instruction file", ArgumentFlag.Optional),
							new (AddressKeyword,	PA,				$"Creates corresponding resource in {Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone).Title} database"),
							new (cdl,				null,			$"Creates dependency links in {Rdn.Rdn.ByZone(Cli.Nexus.Settings.Zone).Title} database", ArgumentFlag.Optional),
							new (depandable,		null,			$"Marks created resource as dependable", ArgumentFlag.Optional),
						];

		a.Examples =	() =>	[
									new (null, @$"{Keyword} {a.Name} {previous}={Urn.Parse(RdnCommand.RZA.Example)} {source}={FILEPATH.Example} {source}={DIRPATH.Example} {manifest}={DIRPATH.Example1}\{AprvAddress.Parse(PA.Example).Version}.{PackageManifest.Extension} {instruction}={DIRPATH.Example1}\{AprvAddress.Parse(PA.Example).Version}.{PackageInstruction.Extension}")
								];

		a.Execute = () =>	{
								var m = Has(manifest) ? File.ReadAllText(GetString(manifest)) : null;
								var i = Has(instruction) ? File.ReadAllText(GetString(instruction)) : null;

								if(m != null)
								{
									var x = PackageManifest.Parse(m);
									x.FillIds(a => rdn.Ppc(new ResourceByAddressPpc(a), Flow).Resource.Id);
									m = x.ToXon(new NetXonTextValueSerializator()).ToString();
								}

								var p = Api<PackageApe>(new PackageBuildApc   
														{
															Sources			= Args.Where(i => i.Name == source).Select(i => i.Get<string>()), 
															Manifest		= m,
															Instruction		= i,
															Previous		= Has(cdl) ? rdn.Ppc(new ResourceByAddressPpc(GetResourceAddress(previous)), Flow).Resource.Id : null, 
															AddressCreator	=	new()
																				{
																					Type = UrnScheme.Hcid,
																					///Owner = GetAccountAddress("owner", null),
																					Resource = address
																				}
														});
								Flow.Log.Dump(p);

								List<Operation> ops = [];

								if(Has(AddressKeyword))
								{
									ops.Add(new ResourceCreation(address, new ResourceData(new DataType(DataType.Self, ContentType.Package_Software_VersionManifest), p.Manifest), Has(depandable)));
								}

								if(Has(cdl))
								{
									var id = Has(AddressKeyword) ? AutoId.LastCreated 
																 : rdn.Ppc(new ResourceByAddressPpc(address), Flow).Resource.Id;
	
									ops.AddRange(p.Manifest.CompleteDependencies.Select(i => new ResourceLinkCreation(id, i.Id, ResourceLinkType.Dependency)));
								}
								
								if(ops.Any())
									Transact(rdn, ops, GetString(ByKeyword), GetLong(BoostKeyword, 0), McvCommand.GetActionOnResult(Args));

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
								var r = Api<PackageApe>(new LocalPackageApc {Id = rdn.Ppc(new ResourceByAddressPpc(address), Flow).Resource.Id});
				
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
							new (AddressKeyword, PA, "Resource address of the package to download")
						];

		a.Execute = () =>	{
								var id = rdn.Ppc(new ResourceByAddressPpc(address), Flow).Resource.Id;

								Api(new StartPackageDownloadApc {Id = id});

								try
								{
									do
									{
										var d = Api<PackageActivityProgress>(new PackageActivityProgressApc {Id = id});
						
										if(d is null)
										{	
											if(!Api<PackageApe>(new LocalPackageApc {Id = id}).Available)
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
								var id = rdn.Ppc(new ResourceByAddressPpc(address), Flow).Resource.Id;

								Api(new PackageDeployApc
									{
										Address = id,
										To = GetString(to, null)}
									);

								try
								{
									do
									{
										var d = Api<PackageActivityProgress>(new PackageActivityProgressApc {Id = id });
						
										if(d is null)
										{	
											if(!Api<PackageApe>(new LocalPackageApc {Id = id}).Available)
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
