using System.Reflection;

namespace Uccs.Fair;

public class AccountCommand : Net.AccountCommand
{
	public AccountCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public override CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new()	{
							Description = "Get account entity information from Ultranet distributed database",
							Arguments = [new (null, AA, "Address of an account to get information about", Flag.First)]
						};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var i = Ppc(new FairAccountRequest(First));
												
								Flow.Log.Dump(i.Account);

								return i.Account;
							};
		return a;
	}
	
	public CommandAction Nickname()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var nickname = "nickname";

		a.Name = "n";
		a.Help = new() {Description = "",
						Arguments =	[new (null, EID,  "Id of account", Flag.First),
									 new (nickname, NAME,  "A new nickname"),
									 SignerArgument()]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AccountNicknameChange {Nickname = GetString(nickname)}; 
							};
		return a;
	}

	public CommandAction ListAuthors()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "la";
		a.Help = new() {Description = "Get authors that specified account owns",
						Arguments = [new (null, AAID, "Id of an account to get authors from", Flag.First)]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new AccountAuthorsRequest(First));

								Flow.Log.Dump(rp.Authors, ["Id"], [i => i]);
					
								return rp.Authors;
							};
		return a;
	}

	public CommandAction ListSites()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "ls";
		a.Help = new() {Description = "Get sites of a specified account",
						Syntax = $"{Keyword} {a.NamesSyntax} {AAID}",
						Arguments = [new (null, AAID, "Id of an account to get sites from", Flag.First)]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new AccountSitesRequest(First));

								Flow.Log.Dump(rp.Sites.Select(i => Ppc(new SiteRequest(i)).Site), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Moderators[0] + (i.Moderators.Length > 1 ? $",  {{{i.Moderators.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Sites;
							};
		return a;
	}
		
	public CommandAction Avatar()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var path = "path";

		a.Name = "avatar";
		a.Help = new() {Description = "Sets avatar for a specified account",
						Arguments =	[new (null, EID, "Id of an author to update"),
									 new (path, PATH, "A path to image file"),
									 SignerArgument()]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return	new AccountAvatarChange {Image = System.IO.File.ReadAllBytes(GetString(path))}; 
							};
		return a;
	}	
}
