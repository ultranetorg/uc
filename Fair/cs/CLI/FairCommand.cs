namespace Uccs.Fair;

public abstract class FairCommand : McvCommand
{
	public readonly ArgumentType AUID		= new ArgumentType("AUID",		@"Author id",																[@"123456-789"]);

	protected FairCli	Program;

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