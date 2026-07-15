using System.Reflection;

namespace Uccs.Rdn.CLI;

public class ReleaseCommand : RdnCommand
{
	//public const string Keyword = "release";

	public ReleaseCommand(RdnCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Deploys a file or files for P2P distribution";
		a.Arguments =	[
							new ("source", PATH, "File or directory paths of content to package", ArgumentFlag.Multi),
						];

		a.Execute = () =>	{
								var a = Api<LocalReleaseApe>(new LocalReleaseBuildApc
															 {	
																Sources = Args.Where(i => i.Name == "source").Select(i => i.Get<string>()),
																AddressCreator = new()	{	
																							Type = GetEnum("addresstype", UrrScheme.Rrrh),
																							//Owner = GetAccountAddress("owner", false),
																							//Resource = Ura.Parse(Args[0].Name)
																						}
															 });

								Report($"Address   : {a.Address}");

								return a;
							};
		return a;
	}

	public CommandAction Local_L()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get information about local copy of the specified release";
		a.Arguments =	[
							AddressArgument(RZA, "release to get information about")
						];

		a.Execute = () =>	{
								var r = Api<LocalReleaseApe>(new LocalReleaseApc {Address = Urr.Parse(Address)});
				
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
