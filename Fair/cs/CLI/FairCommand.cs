namespace Uccs.Fair.CLI;

public abstract class FairCommand : McvCommand
{
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