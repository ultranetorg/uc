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
		a.Help = new() {Description = "Get account entity information from Ultranet distributed database",
						Syntax = $"{Keyword} {a.NamesSyntax} {AA}",

						Arguments = [new ("<first>", "Address of an account to get information about")],

						Examples = [new (null, $"{Keyword} {a.Name} {AA.Example}")]};

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
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {nickname}={NAME} {SignerArg}={AA}",

						Arguments =	[new (nickname, "A new nickname"),
									 new (SignerArg, "Address of account")],

						Examples =	[new (null, $"{Keyword} {a.Name} {nickname}={NAME.Example} {SignerArg}={AA.Example}")]};

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

						Syntax = $"{Keyword} {a.NamesSyntax} {AAID}",
					
						Arguments = [new ("<first>", "Id of an account to get authors from")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example}"),
									 new (null, $"{Keyword} {a.Name} {AA.Example}")]};

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

						Arguments = [new ("<first>", "Id of an account to get sites from")],

						Examples = [new (null, $"{Keyword} l {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new AccountSitesRequest(First));

								Flow.Log.Dump(rp.Sites.Select(i => Ppc(new SiteRequest(i)).Site), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Moderators[0] + (i.Moderators.Length > 1 ? $",  {{{i.Moderators.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Sites;
							};
		return a;
	}

}
