using System.Reflection;

namespace Uccs.Net;

public class UserCommand : McvCommand
{
	new protected AutoId Id
	{
		get
		{
			if(Has(IdKeyword))
				return GetAutoId(IdKeyword);
			else if(Has(NameKeyword))
				return Ppc(new UserPpc(Name)).User.Id;
			else
				throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");
		}
	}

	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public UserCommand()
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

	public CommandAction Create_C()
	{
		const string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

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

	public CommandAction Name_N()
	{
		const string name = nameof(name);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

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

	public CommandAction Security_S()
	{
		const string owner = nameof(owner);

		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

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

	public CommandAction AllocateBandwidth_AB()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string bandwidth = nameof(bandwidth);
		const string months = nameof(months);

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
	
	public CommandAction Membership_M()
	{ 
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		a.Description = "Get information about membership status of specified user";
		a.Arguments =	[
							NameOrId("user to check membership status of")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var rp = Ppc(new MembersPpc());
	
								var id = Id;

								var m = rp.Members.FirstOrDefault(i => i.User == id)
										??
										throw new EntityException(EntityError.NotFound);

								Flow.Log.Dump(m);

								return m;
							};

		return a;
	}

	public virtual CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about the specified user";
		a.Arguments = [NameOrId("user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								User u;

								if(Has(IdKeyword))
									u = Ppc(new UserPpc(Id)).User;
								else if(Has(NameKeyword))
									u = Ppc(new UserPpc(Name)).User;
								else
									throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");

								Flow.Log.Dump(u);

								return u;
							};
		return a;
	}
}
