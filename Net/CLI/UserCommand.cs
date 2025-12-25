using System.Reflection;

namespace Uccs.Net;

public class UserCommand : McvCommand
{
	protected string		First => Args[0].Name;

	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var name = "name";
		var owner = "owner";

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Create a new account entity";
		a.Arguments = [	new (name,	NAME, "User name"),
						new (owner, AA, "Account public address of account owner")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								return new UserCreation {Name = GetString(name), Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public virtual CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get account entity information from MCV database";
		a.Arguments = [new ("name", NAME, "A name of user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Ppc(new UserPpc(GetString(a.Arguments[0].Name)));
												
								Flow.Log.Dump(i.User);

								return i.User;
							};
		return a;
	}
}
