namespace Uccs.Uos;

/// <summary>
/// Usage: 
///		
/// </summary>

public class PackageCommand : UosCommand
{
	public const string Keyword = "package";
	Ura					Package => Ura.Parse(Args[0].Name);

	public PackageCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help
							{ 
								Title = "CREATE",
								Description = "Builds and deploys a package to a node filebase for distribution via RDN",
								Syntax = "package c|create PACKAGE_ADDRESS [sources=PATH,PATH,...,PATH] dependencies=PATH previous=PACKAGE_ADDRESS",

								Arguments =	[
												new ("<first>", "Resource address of package to create"),
												new ("sources", "A list of paths to files separated by comma"),
												new ("dependencies", "A path to manifest file where complete dependencies are defined"),
												new ("previous", "Address of previous release")
											],

								Examples =
								[
									new (null, "package c company/windows/application/0.0.2 previous=company/windows/application/0.0.1 sources=C:\\application.exe,C:\\changelog.txt,C:\\logo.jpg dependencies=C:\\product\\1.2.3.manifest")
								]
							},

							Execute = () =>	{
												Ura p = null;
												VersionManifest m = null;

												var r = RdnRequest<LocalReleaseApe>(new PackageBuildApc{	Resource		 = Ura.Parse(Args[0].Name), 
																										Sources			 = GetString("sources").Split(','), 
																										DependenciesPath = GetString("dependencies", false),
																										Previous		 = GetResourceAddress("previous", false),
																										AddressCreator	 = new(){	Type = GetEnum("addresstype", UrrScheme.Urrh),
																																	Owner = GetAccountAddress("owner", false),
																																	Resource = Ura.Parse(Args[0].Name)} });
												Dump($"Address : {r}");

												return r;
											}
						},

						new ()
						{
							Names = ["l", "local"],

							Help = new Help
							{ 
								Title = "LOCAL",
								Description = "Gets information about local copy of a specified package",
								Syntax = "package l|local PACKAGE_ADDRESS",

								Arguments =	[
												new ("<first>", "Address of local package to get information about")
											],

								Examples =	[
												new (null, "package l company/application/windows/1.2.3")
											]
							},

							Execute = () =>	{
												var r = RdnRequest<PackageInfo>(new LocalPackageApc {Address = Package});
				
												Dump(r);

												return null;
											}
						},

						new ()
						{
							Names = ["d", "download"],

							Help = new Help
							{ 
								Title = "DOWNLOAD",
								Description = "Downloads a specified package by its address",
								Syntax = "package d|download PACKAGE_ADDRESS",

								Arguments =	[
												new ("<first>", "Address of a package to download")
											],

								Examples =	[
												new (null, "package d company/application/windows/1.2.3")
											]
							},

							Execute = () =>	{
												RdnSend(new PackageDownloadApc {Package = Package});

												try
												{
													do
													{
														var d = RdnRequest<ResourceActivityProgress>(new PackageActivityProgressApc {Package = Package});
						
														if(d is null)
														{	
															if(!RdnRequest<PackageInfo>(new LocalPackageApc {Address = Package}).Available)
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
											}
						},

						new ()
						{
							Names = ["dp", "deploy"],

							Help = new Help
							{ 
								Title = "DEPLOY",
								Description = "If needed, downloads specified package and its dependencies recursively and deploys its content to the 'Packages' directory",
								Syntax = $"{Keyword} i|install PACKAGE_ADDRESS destination=PATH",

								Arguments =	[
												new ("<first>", "Address of a package to install"),
												new ("destination", "Packages destination path")
											],

								Examples =	[
												new (null, $"{Keyword} deploy company/application/windows/1.2.3")
											]
							},

							Execute = () =>	{
												RdnSend(new PackageDeployApc{	Address = AprvAddress.Parse(Args[0].Name),
																				DeploymentPath = GetString("destination", null)});

												try
												{
													do
													{
														var d = RdnRequest<ResourceActivityProgress>(new PackageActivityProgressApc {Package = Package});
						
														if(d is null)
														{	
															if(!RdnRequest<PackageInfo>(new LocalPackageApc {Address = Package}).Available)
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
											}
						}
					];
	}

	protected Ura GetResourceAddress(string paramenter, bool mandatory = true)
	{
		if(Has(paramenter))
			return Ura.Parse(GetString(paramenter));
		else
			if(mandatory)
				throw new SyntaxException($"Parameter '{paramenter}' not provided");
			else
				return null;
	}
}
