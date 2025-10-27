using System.Reflection;

namespace Uccs.Rdn.CLI;

public class PackageCommand : RdnCommand
{
	Ura	Package => Ura.Parse(Args[0].Name);

	public readonly ArgumentType PA 	= new ArgumentType("PA", "Package resource address", [@"company/application/windows/1.2.3"]);
	public readonly ArgumentType REALA 	= new ArgumentType("RLSTA", "Realization address", [@"company/application/windows"]);

	public PackageCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new (){Description = "Builds and deploys a package to a node filebase for distribution via RDN",
						Syntax = $"{Keyword} {a.NamesSyntax} {PA} [sources={PATH},{PATH}...{PATH}] dependencies={FILEPATH} previous={PA}",

						Arguments =	[
										new (FirstArg, "Resource address of package to create"),
										new ("sources", "A list of paths to files separated by comma"),
										new ("dependencies", "A path to version manifest file where complete dependencies are defined"),
										new ("previous", "Address of previous release")
									],

						Examples =	[
										new (null, @$"{Keyword} {a.Name} {REALA}/0.0.2 previous={REALA}/0.0.1 sources={FILEPATH.Example[0]},{FILEPATH.Example[1]},{FILEPATH.Example[2]} dependencies={DIRPATH}\1.2.3.{VersionManifest.Extension}")
									]};

		a.Execute = () =>	{
								var r = Api<LocalReleaseApe>(new PackageBuildApc   {Resource		 = Ura.Parse(Args[0].Name), 
																					Sources			 = GetString("sources").Split(','), 
																					DependenciesPath = GetString("dependencies", false),
																					Previous		 = GetResourceAddress("previous", false),
																					AddressCreator	 = new(){	Type = GetEnum("addresstype", UrrScheme.Urrh),
																												Owner = GetAccountAddress("owner", false),
																												Resource = Ura.Parse(Args[0].Name)} });
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
						Syntax = $"{Keyword} {a.NamesSyntax} {PA}",

						Arguments =	[
										new (FirstArg, "Address of local package to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {PA.Example}")
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
						Syntax = $"{Keyword} {a.NamesSyntax} {PA}",

						Arguments =	[
										new (FirstArg, "Address of a package to download")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {PA.Example}")
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
			Syntax = $"{Keyword} {a.NamesSyntax} {PA} destination={DIRPATH}",

			Arguments =	[
							new (FirstArg, "Address of a package to install"),
							new ("destination", "Packages destination path")
						],

			Examples =	[
							new (null, $"{Keyword} {a.Name} {PA.Example}")
						]
		};

		a.Execute = () =>	{
								Api(new PackageDeployApc {Address = ApvAddress.Parse(Args[0].Name),
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
