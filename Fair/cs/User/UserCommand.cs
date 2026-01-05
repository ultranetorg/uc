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

		a.Name = "e";
		a.Description = "Get account entity information from MCV distributed database";
		a.Arguments = [new ("name", NAME, "A name of user to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Ppc(new FairUserPpc(GetString(a.Arguments[0].Name)));
												
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
//		a.Description = "Sets a nickname for a specified account";
//		a.Arguments =  [new (null, EID,  "Id of account", Flag.First),
//						new (nickname, NAME,  "A new nickname"),
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

		a.Name = "la";
		a.Description = "Get authors that specified account owns";
		a.Arguments = [new ("name", NAME, "A name of user to get authors from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new UserAuthorsPpc(GetString(a.Arguments[0].Name)));

								Flow.Log.Dump(rp.Authors, ["Id"], [i => i]);
					
								return rp.Authors;
							};
		return a;
	}

	public CommandAction ListSites()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "ls";
		a.Description = "Get sites of a specified account";
		a.Arguments = [new ("name", NAME, "A name of user to get sites from")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new UserSitesPpc(GetString(a.Arguments[0].Name)));

								Flow.Log.Dump(rp.Sites.Select(i => Ppc(new SitePpc(i)).Site), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Moderators[0] + (i.Moderators.Length > 1 ? $",  {{{i.Moderators.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Sites;
							};
		return a;
	}
		
	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var path = "path";

		a.Name = "avatar";
		a.Description = "Sets an avatar for a specified account";
		a.Arguments =  [new (path, PATH, "A path to image file"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return	new UserAvatarChange {Image = System.IO.File.ReadAllBytes(GetString(path))}; 
							};
		return a;
	}	
}
