using System.Reflection;

namespace Uccs.Net;

public class UserCommand : McvCommand
{
	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	//public CommandAction Create()
	//{
	//	var name = "name";
	//	var owner = "owner";
	//
	//	var a = new CommandAction(this, MethodBase.GetCurrentMethod());
	//
	//	a.Name = "c";
	//	a.Description = "Create a new user entity";
	//	a.Arguments = [	new (name,	NAME, "User name"),
	//					new (owner, AA, "Account public address of account owner")];
	//
	//	a.Execute = () =>	{
	//							Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
	//
	//							return new UserCreation {Name = GetString(name), Owner = GetAccountAddress(owner)};
	//						};
	//	return a;
	//}

	public CommandAction Create()
	{
		string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Creates a new user for free. Uses POW.";
		a.Arguments =	[
							new (owner, AA, "Account address that will have access to the user to be created"),
							ByArgument("The name of the user to be created")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return new UserCreation {Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public CommandAction UpdateName()
	{
		string name = nameof(name);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "un";
		a.Description = "Sets a new name for the user";
		a.Arguments	  =	[
							new (name, NAME, "A new user name"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return new UserNameChange {Name = GetString(name)};
							};
		return a;
	}

	public CommandAction Security()
	{
		string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "s";
		a.Description = "Creates a new account entity";
		a.Arguments =	[
							new (owner, AA, "Public address of a new account owner"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return new UserOwnerChange {Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public CommandAction AllocateBandwidth()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Name = "ab";
		a.Description = "Allocates execution bandwidth";
		a.Arguments =	[
							new ("bandwidth", EC, "Amount of energy allocated per hour"),
							new ("months", INT, "Number of months to allocate bandwidth for"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new UserBandwidthAllocation {Bandwidth = GetUInt16("bandwidth"), Months = GetByte("months")};
							};

		return a;
	}

	public virtual CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Gets account entity information from MCV database";
		a.Arguments = [new ("name", NAME, "The name of the user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var i = Ppc(new UserPpc(GetString(a.Arguments[0].Name)));
												
								Flow.Log.Dump(i.User);

								return i.User;
							};
		return a;
	}
}
