using System.Reflection;

namespace Uccs.Smp;

public class AccountCommand : McvCommand
{
	AccountIdentifier		First => AccountIdentifier.Parse(Args[0].Name);

	public AccountCommand(McvCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
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
				
								var rp = Rdc(new AccountAuthorsRequest(First));

								Dump(rp.Authors, ["Id"], [i => i]);
					
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
				
								var rp = Rdc(new AccountSitesRequest(First));

								Dump(rp.Sites.Select(i => Rdc(new SiteRequest(i)).Site), ["Id", "Title", "Owners", "Root Categories"], [i => i.Id, i => i.Title, i => i.Owners[0] + (i.Owners.Length > 1 ? $",  {{{i.Owners.Length-1}}} more" : null), i => i.Categories?.Length]);
					
								return rp.Sites;
							};
		return a;
	}

}
