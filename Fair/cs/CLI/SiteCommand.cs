using System.Reflection;

namespace Uccs.Fair;

public class SiteCommand : FairCommand
{
	EntityId FirstSiteId => EntityId.Parse(Args[0].Name);

	public SiteCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a new site",
						Syntax = $"{Keyword} {a.NamesSyntax} years={INT} {SignerArg}={AA}",
						
						Arguments =	[new ("years", "Integer number of years in [1..10] range"),
									 new (SignerArg, "Address of account that owns or is going to register the site")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
							Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

							return new SiteCreation {Title = GetString("title")};
						};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get site entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments =	[new ("<first>", "Id of an site to get information about")],
						Examples =[new (null, $"{Keyword} e {EID.Example}")]};

		a.Execute = () =>	{
							Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
							var rp = Rdc(new SiteRequest(FirstSiteId));

							Dump(rp.Site);
					
							return rp.Site;
						};
		return a;
	}

	public CommandAction List()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Get sites of a specified account",
						Syntax = $"{Keyword} {a.NamesSyntax} {AAID}",
						Arguments = [new ("<first>", "Id of an account to get sites from")],
						Examples = [new (null, $"{Keyword} l {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new AccountSitesRequest(AccountIdentifier.Parse(Args[0].Name)));

								Dump(rp.Sites.Select(i => Rdc(new SiteRequest(i)).Site), ["Id", "Title", "Team", "Cards"], [i => i.Id, i => i.Title, i => i.Owners.Length, i => i.Roots?.Length]);
					
								return rp.Sites;
							};
		return a;
	}
}
