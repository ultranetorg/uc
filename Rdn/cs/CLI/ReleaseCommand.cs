using System.Reflection;

namespace Uccs.Rdn.CLI;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class ReleaseCommand : RdnCommand
{
	//public const string Keyword = "release";

	public ReleaseCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction CREATE()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Deploys a file or files for P2P distribution",
						Syntax = $"{Keyword} {a.NamesSyntax} [source={PATH} | sources={PATH},{PATH},...,{PATH}]",

						Arguments =	[
										new ("source", "A path to a file to build"),
										new ("sources", "A list of paths to files separated by comma")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} sources={PATH.Examples[0]},{PATH.Examples[1]},{PATH.Examples[2]}")
									]};

		a.Execute = () =>	{
								if(!Has("source") && !Has("sources"))
									throw new SyntaxException("Unknown arguments");

								var a = Api<LocalReleaseApe>(new LocalReleaseBuildApc {	Source = GetString("source", null),
																						Sources = GetString("sources", null)?.Split(','),
																						AddressCreator = new()	{	
																													Type = GetEnum("addresstype", UrrScheme.Urrh),
																													//Owner = GetAccountAddress("owner", false),
																													//Resource = Ura.Parse(Args[0].Name)
																												} });

								Report($"Address   : {a}");

								return a;
							};
		return a;
	}

	public CommandAction LOCAL()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Get information about local copy of a specified release",
						Syntax = $"{Keyword} {a.NamesSyntax} {RZA}",

						Arguments =	[
										new ("<first>", "Address of a release to get information about")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {RZA.Example}")
									]};

		a.Execute = () =>	{
								var r = Api<LocalReleaseApe>(new LocalReleaseApc {Address = Urr.Parse(Args[0].Name)});
				
								if(r != null)
								{
									Dump(r);
									return r;
								}
								else
									throw new Exception("Resource not found");
							};
		return a;
	}
}
