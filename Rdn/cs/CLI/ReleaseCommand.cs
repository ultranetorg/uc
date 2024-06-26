namespace Uccs.Rdn.CLI
{
	/// <summary>
	/// Usage: 
	///		release publish 
	/// </summary>
	public class ReleaseCommand : RdnCommand
	{
		public const string Keyword = "release";

		public ReleaseCommand(Program program, List<Xon> args, Flow flow) : base(program, args, flow)
		{
			Actions =	[
							new ()
							{
								Names = ["c", "create"],

								Help = new Help
								{ 
									Title = "CREATE",
									Description = "Deploy a file or files to node filebase for P2P distribution via RDN",
									Syntax = "release c|create [source=PATH | sources=PATH,PATH,...,PATH]",

									Arguments =
									[
										new ("source", "A path to a file to build"),
										new ("sources", "A list of paths to files separated by comma")
									],

									Examples =
									[
										new (null, "release c company/product sources=C:\\application.exe,C:\\changelog.txt,C:\\logo.jpg")
									]
								},

								Execute = () =>	{
													if(!Has("source") && !Has("sources"))
														throw new SyntaxException("Unknown arguments");

													var a = Api<Urr>(new ReleaseBuildApc {	Source = GetString("source", null),
																							Sources = GetString("sources", null)?.Split(','),
																							AddressCreator = new()	{	
																														Type = GetEnum("addresstype", UrrScheme.Urrh),
																														Owner = GetAccountAddress("owner", false),
																														Resource = Ura.Parse(Args[0].Name)
																													} });

													Report($"Address   : {a}");

													return a;
												}
							},

							new ()
							{
								Names = ["l", "local"],

								Help = new Help
								{ 
									Title = "LOCAL",
									Description = "Get information about local copy of a specified release",
									Syntax = "release l|local RELEASE_ADDRESS",

									Arguments =
									[
										new ("<first>", "Address of a release to get information about")
									],

									Examples =
									[
										new (null, "release l updh:F371BC4A311F2B009EEF952DD83CA80E2B60026C8E935592D0F9C308453C813E")
									]
								},

								Execute = () =>	{
													var r = Api<LocalReleaseApc.Return>(new LocalReleaseApc {Address = Urr.Parse(Args[0].Name)});
					
													if(r != null)
													{
														Dump(r);
														return r;
													}
													else
														throw new Exception("Resource not found");
												}
							},
						];
		}
	}
}
