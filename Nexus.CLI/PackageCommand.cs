using System.Reflection;
using Uccs.Rdn;

namespace Uccs.Nexus;

public class PackageCommand : NexusCommand
{
	Ura	Package => Ura.Parse(Args[0].Name);

	public readonly ArgumentType PA 	= new ArgumentType("PA", "Package resource address", [@"company/application/windows/1.2.3"]);
	public readonly ArgumentType RZA 	= new ArgumentType("RLSTA", "Realization address", [@"company/application/windows"]);

	public PackageCommand(NexusCli cli, List<Xon> args, Flow flow) : base(cli, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new (){Description = "Builds and deploys a package to a node filebase for distribution via RDN",
						Arguments =	[
										new (null, PA, "Resource address of package to create", Flag.First),
										new ("source", PATH, "A list of paths to files separated by comma", Flag.Multi),
										new ("dependencies", FILEPATH, "A path to version manifest file where complete dependencies are defined", Flag.Optional),
										new ("previous", PA, "Address of previous release", Flag.Optional)
									],

						Examples =	[
										new (null, @$"{Keyword} {a.Name} {RZA}/0.0.2 previous={RZA}/0.0.1 source={FILEPATH.Example[0]} source{FILEPATH.Example[1]} dependencies={DIRPATH}\1.2.3.{VersionManifest.Extension}")
									]};

		a.Execute = () =>	{
								var r = Api<LocalReleaseApe>(new PackageBuildApc   {Resource		 = Ura.Parse(Args[0].Name), 
																					Sources			 = Args.Where(i => i.Name == "source").Select(i => i.Get<string>()), 
																					DependenciesPath = GetString("dependencies", false),
																					Previous		 = GetResourceAddress("previous", false),
																					AddressCreator	 =	new()
																										{
																											Type = GetEnum("addresstype", UrrScheme.Urrh),
																											Owner = GetAccountAddress("owner", false),
																											Resource = Ura.Parse(Args[0].Name)
																										}});
								Flow.Log.Dump($"Address : {r}");

								return r;
							};
		return a;
	}

	public CommandAction Local()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Gets information about local copy of a specified package",
						Arguments =	[
										new (null, PA, "Address of local package to get information about", Flag.First)
									]};

		a.Execute = () =>	{
							var r = Api<PackageInfo>(new LocalPackageApc {Address = Package});
				
							Flow.Log.Dump(r);

							return null;
						};
		return a;
	}

	public CommandAction Download()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "d";
		a.Help = new() {Description = "Downloads a specified package by its address",
						Arguments =	[
										new (null, PA, "Address of a package to download", Flag.First)
									]};

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
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "dp";
		a.Help = new ()
				 { 
					Description = "If needed, downloads specified package and its dependencies recursively and deploys its content to the 'Packages' directory",
					Arguments =	[
									new (null, PA, "Address of a package to install", Flag.First),
									new ("destination", DIRPATH, "Packages destination path", Flag.Optional)
								],
				 };

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
