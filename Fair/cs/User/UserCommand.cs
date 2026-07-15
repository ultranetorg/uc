using System.Reflection;

namespace Uccs.Fair;

public class UserCommand : Net.UserCommand
{
	public UserCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
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

	public CommandAction ListAuthors_LA()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get authors owned by the specified user";
		a.Arguments = [NameOrId(NAME, "user to get authors from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new UserAuthorsPpc(Id));

								Flow.Log.Dump(rp.Authors, ["Id"], [i => i]);
					
								return rp.Authors;
							};
		return a;
	}

	public CommandAction ListStores_LS()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string name = nameof(name);

		a.Description = "Get sites of the specified user";
		a.Arguments = [NameOrId(NAME, "user to get stores from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new UserStoresPpc(Id));

								Flow.Log.Dump(rp.Stores.Select(i => Ppc(new StorePpc(i)).Store), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Moderators[0] + (i.Moderators.Length > 1 ? $",  {{{i.Moderators.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Stores;
							};
		return a;
	}
		
	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string path = nameof(path);

		a.Description = "Sets an avatar for the signing user";
		a.Arguments =  [new (path, PATH, "A path to image file"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var data = System.IO.File.ReadAllBytes(GetString(path));

								return	new UserAvatarChange {Image = data}; 
							};
		return a;
	}	

	public override CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get information about the user specified";
		a.Arguments = [NameOrId(NAME, "user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								User u;

								if(Has(IdKeyword))
									u = Ppc(new FairUserPpc(Id)).User;
								else if(Has(NameKeyword))
									u = Ppc(new FairUserPpc(Name)).User;
								else
									throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");
																					
								Flow.Log.Dump(u);

								return u;
							};
		return a;
	}
}
