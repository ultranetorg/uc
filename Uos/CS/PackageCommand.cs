using System.Reflection;

namespace Uccs.Uos;

public class PackageCommand : UosCommand
{
	Ura	Package => Ura.Parse(Args[0].Name);

	public readonly ArgumentType PA 	= new ArgumentType("PA", "Package resource address", [@"company/application/windows/1.2.3"]);
	public readonly ArgumentType REALA 	= new ArgumentType("RLSTA", "Realizattion address", [@"company/application/windows"]);

	public PackageCommand(Uos uos, List<Xon> args, Flow flow) : base(uos, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new (){Description = "Builds and deploys a package to a node filebase for distribution via RDN",
						Syntax = $"{Keyword} {a.NamesSyntax} {PA} [sources={PATH},{PATH}...{PATH}] dependencies={FILEPATH} previous={PA}",

						Arguments =	[
										new ("<first>", "Resource address of package to create"),
										new ("sources", "A list of paths to files separated by comma"),
										new ("dependencies", "A path to version manifest file where complete dependencies are defined"),
										new ("previous", "Address of previous release")
									],

						Examples =	[
										new (null, @$"{Keyword} {a.Name} {REALA}/0.0.2 previous={REALA}/0.0.1 sources={FILEPATH.Example[0]},{FILEPATH.Example[1]},{FILEPATH.Example[2]} dependencies={DIRPATH}\1.2.3.{VersionManifest.Extension}")
									]};

		a.Execute = () =>	{
								Ura p = null;
								VersionManifest m = null;

								var r = RdnRequest<LocalReleaseApe>(new PackageBuildApc{Resource		 = Ura.Parse(Args[0].Name), 
																						Sources			 = GetString("sources").Split(','), 
																						DependenciesPath = GetString("dependencies", false),
																						Previous		 = GetResourceAddress("previous", false),
																						AddressCreator	 = new(){	Type = GetEnum("addresstype", UrrScheme.Urrh),
																													Owner = GetAccountAddress("owner", false),
																													Resource = Ura.Parse(Args[0].Name)} });
								Dump($"Address : {r}");

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
										new ("<first>", "Address of local package to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {PA.Example}")
									]};

		a.Execute = () =>	{
							var r = RdnRequest<PackageInfo>(new LocalPackageApc {Address = Package});
				
							Dump(r);

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
										new ("<first>", "Address of a package to download")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {PA.Example}")
									]};

		a.Execute = () =>	{
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
							new ("<first>", "Address of a package to install"),
							new ("destination", "Packages destination path")
						],

			Examples =	[
							new (null, $"{Keyword} {a.Name} {PA.Example}")
						]
		};

		a.Execute = () =>	{
							RdnSend(new PackageDeployApc {Address = AprvAddress.Parse(Args[0].Name),
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
						};
		return a;
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
