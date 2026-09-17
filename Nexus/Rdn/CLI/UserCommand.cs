using System.Reflection;

namespace Uccs.Rdn.CLI;

public class UserCommand : Net.UserCommand
{
	public UserCommand() : base()
	{
	}

	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}
	
	public CommandAction Name_N()
	{
		const string newname = nameof(newname);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Sets a new name for the user";
		a.Arguments	  =	[
							new (newname, NAME, "New user name"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return new UserRenaming {Name = GetString(newname)};
							};
		return a;
	}

}
