using System.Reflection;

namespace Uccs.Fair;

public abstract class FairCommand : McvCommand
{
	protected FairCli				Program;
	//public readonly ArgumentType	SECURITY = new ArgumentType("SECURITY",	
	//															@"Page security definition in form of {Change0=Actor,Actor,Actor... Change1=Actor,Actor,Actor...}",
	//															[$"{{{TopicChange.AddPages}={Actor.Owner},{Actor.SiteUser} {TopicChange.Security}={Actor.Owner}}}"]);
	//
	public readonly ArgumentType	EA = new ArgumentType("EA",	@"Entity Address", [$"{FairTable.Account}/123-456"]);
	protected AutoId				FirstEntityId => AutoId.Parse(Args[0].Name);
	protected AutoId				SecondEntityId => AutoId.Parse(Args[1].Name);

	protected string				As = "as";

	public readonly ArgumentType	ROLE = new ArgumentType("ROLE",	@"Site role", Enum.GetNames<Uccs.Fair.Role>().ToArray());

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