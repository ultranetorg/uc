namespace Uccs.Fair;

public abstract class FairCommand : McvCommand
{
	protected FairCli				Program;
	//public readonly ArgumentType	SECURITY = new ArgumentType("SECURITY",	
	//															@"Page security definition in form of {Change0=Actor,Actor,Actor... Change1=Actor,Actor,Actor...}",
	//															[$"{{{TopicChange.AddPages}={Actor.Owner},{Actor.SiteUser} {TopicChange.Security}={Actor.Owner}}}"]);
	//
	public readonly ArgumentType	SITETYPE = new ArgumentType("SITETYPE",	@"Site type", [$"{SiteType.Store}"]);

	static FairCommand()
	{
		try
		{
			var p = Console.KeyAvailable;
			ConsoleAvailable = true;
		}
		catch(Exception)
		{
			ConsoleAvailable = false;
		}
	}

	protected FairCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Program = program;
	}
}