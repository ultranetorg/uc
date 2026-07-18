using System.Reflection;

namespace Uccs.Fair;

public abstract class FairCommand : McvCommand
{
	protected FairCli						Program;

	protected const string					AsKeyword = "as";

	public static readonly ArgumentType		PRODUCTTYPE = new ArgumentType("PRODUCTTYPE",	@"Product Type", [$"{ProductType.Software}"]);
	public static readonly ArgumentType		ROLE = new ArgumentType("ROLE",	@"Store role", Enum.GetNames<Uccs.Fair.Role>().ToArray());
	public static readonly ArgumentType		HYPERLINK = new ArgumentType("HYPERLINK", "{text=TEXT uri=URI}", ["{text=Website uri=http://www.company.com}", "{text=Github uri=http://github.com/company}"]);

	protected FairCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Program = program;
	}
}