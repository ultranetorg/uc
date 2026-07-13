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
		const string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Creates a new user for free. Uses POW.";
		a.Arguments =	[
							new (owner, AA, "Account address that has access to the user being created"),
							ByArgument("Name of the user to be created")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								return new UserCreation {Owner = GetAccountAddress(owner)};
							};
		return a;
	}

	public CommandAction Name()
	{
		const string name = nameof(name);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "n";
		a.Description = "Sets a new name for the user";
		a.Arguments	  =	[
							new (name, NAME, "New user name"),
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
		const string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "s";
		a.Description = "Manages the security settings for the specified user";
		a.Arguments =	[
							new (owner, AA, "Public address of the new account owner"),
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
		
		const string bandwidth = nameof(bandwidth);
		const string months = nameof(months);

		a.Name = "ab";
		a.Description = "Allocates execution bandwidth";
		a.Arguments =	[
							new (bandwidth,	EC, "Amount of energy allocated per hour"),
							new (months,	INT, "Number of months to allocate bandwidth for"),
							ByArgument()
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new UserBandwidthAllocation {Bandwidth = GetUInt16(bandwidth), Months = GetByte(months)};
							};

		return a;
	}

	public virtual CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string name = nameof(name);

		a.Name = "e";
		a.Description = "Gets information about the specified user";
		a.Arguments = [new (name, NAME, "Name of the user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var i = Ppc(new UserPpc(GetString(name)));
												
								Flow.Log.Dump(i.User);

								return i.User;
							};
		return a;
	}
}
