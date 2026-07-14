using System.Reflection;

namespace Uccs.Fair;

public class UserCommand : Net.UserCommand
{
	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public override CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string name = nameof(name);

		a.Name = "e";
		a.Description = "Get information about the user specified";
		a.Arguments = [new (name, NAME, "Name of the user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var i = Ppc(new FairUserPpc(GetString(name)));
												
								Flow.Log.Dump(i.User);

								return i.User;
							};
		return a;
	}
	
//	public CommandAction Nickname()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//		
//		var nickname = "nickname";
//
//		a.Name = "n";
//		a.Description = "Sets a nickname for the specified account";
//		a.Arguments =  [new (null, EID,  "Id of account", ArgumentFlag.First),
//						new (nickname, NAME,  "New nickname"),
//						SignerArgument()];
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
//
//								return new AccountNicknameChange {Nickname = GetString(nickname)}; 
//							};
//		return a;
//	}

	public CommandAction ListAuthors()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string name = nameof(name);

		a.Name = "la";
		a.Description = "Get authors that specified account owns";
		a.Arguments = [new (name, NAME, "Name of the user to get authors from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new UserAuthorsPpc(GetString(name)));

								Flow.Log.Dump(rp.Authors, ["Id"], [i => i]);
					
								return rp.Authors;
							};
		return a;
	}

	public CommandAction ListStores()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string name = nameof(name);

		a.Name = "ls";
		a.Description = "Get sites of the specified user";
		a.Arguments = [new (name, NAME, "Name of the user to get stores from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new UserStoresPpc(GetString(name)));

								Flow.Log.Dump(rp.Stores.Select(i => Ppc(new StorePpc(i)).Store), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Moderators[0] + (i.Moderators.Length > 1 ? $",  {{{i.Moderators.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Stores;
							};
		return a;
	}
		
	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string path = nameof(path);

		a.Name = "avatar";
		a.Description = "Sets an avatar for the specified user";
		a.Arguments =  [new (path, PATH, "A path to image file"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var data = System.IO.File.ReadAllBytes(GetString(path));

								return	new UserAvatarChange {Image = data}; 
							};
		return a;
	}	
}
