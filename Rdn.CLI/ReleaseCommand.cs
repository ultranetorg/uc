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

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Deploys a file or files for P2P distribution";
		a.Arguments =	[
							new ("source", PATH, "A path to a file to build", Flag.Multi),
						];

		a.Execute = () =>	{
								var a = Api<LocalReleaseApe>(new LocalReleaseBuildApc
															 {	
																Sources = Args.Where(i => i.Name == "source").Select(i => i.Get<string>()),
																AddressCreator = new()	{	
																							Type = GetEnum("addresstype", UrrScheme.Urrh),
																							//Owner = GetAccountAddress("owner", false),
																							//Resource = Ura.Parse(Args[0].Name)
																						}
															 });

								Report($"Address   : {a.Address}");

								return a;
							};
		return a;
	}

	public CommandAction Local()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Description = "Get information about local copy of a specified release";
		a.Arguments =	[
							new (null, RZA, "Address of a release to get information about")
						];

		a.Execute = () =>	{
								var r = Api<LocalReleaseApe>(new LocalReleaseApc {Address = Urr.Parse(Args[0].Name)});
				
								if(r != null)
								{
									Flow.Log.Dump(r);
									return r;
								}
								else
									throw new Exception("Resource not found");
							};
		return a;
	}
}
