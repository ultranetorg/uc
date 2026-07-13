using System.Reflection;

namespace Uccs.Fair;

public abstract class FairCommand : McvCommand
{
	protected FairCli						Program;
	//public readonly ArgumentType			SECURITY = new ArgumentType("SECURITY",	
	//																	@"Page security definition in form of {Change0=Actor,Actor,Actor... Change1=Actor,Actor,Actor...}",
	//																	[$"{{{TopicChange.AddPages}={Actor.Owner},{Actor.StoreUser} {TopicChange.Security}={Actor.Owner}}}"]);
	//
	protected AutoId						FirstAutoId => AutoId.Parse(Args[0].Name);
	protected AutoId						SecondAutoId => AutoId.Parse(Args[1].Name);

	protected string						As = "as";

	public static readonly ArgumentType		PRODUCTTYPE = new ArgumentType("PRODUCTTYPE",	@"Product Type", [$"{ProductType.Software}"]);
	public static readonly ArgumentType		ROLE = new ArgumentType("ROLE",	@"Store role", Enum.GetNames<Uccs.Fair.Role>().ToArray());

	protected FairCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Program = program;
	}
}