namespace Uccs.Fair;

public class SiteCommand : FairCommand
{
	public const string Keyword = "site";

	EntityId FirstSiteId => EntityId.Parse(Args[0].Name);

	public SiteCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help()
							{
								Title = "Create",
								Description = "Creates a new site",
								Syntax = $"{Keyword} c|create title={TITLE} {SignerArg}={AA}",
								Arguments =	[new ("years", "Integer number of years in [1..10] range"),
											 new (SignerArg, "Address of account that owns or is going to register the site")],
								Examples =	[new (null, $"{Keyword} c title=\"The Store\" {SignerArg}=0x0000fffb3f90771533b1739480987cee9f08d754")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new SiteCreation {Title = GetString("title")};
											}
						},

						new ()
						{
							Names = ["e", "entity"],

							Help = new Help()
							{
								Title = "Entity",
								Description = "Get site entity information from MCV database",
								Syntax = $"{Keyword} e|entity {EID}",
								Arguments =	[new ("<first>", "Id of an site to get information about")],
								Examples =[new (null, $"{Keyword} e {EID.Examples[0]}")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new SiteRequest(FirstSiteId));

												Dump(rp.Site);
					
												return rp.Site;
											}
						},

						new ()
						{
							Names = ["l", "list"],

							Help = new Help {Title = "List",
											 Description = "Get sites of a specified account",
											 Syntax = $"{Keyword} l|list {AAID}",
											 Arguments = [new ("<first>", "Id of an account to get sites from")],
											 Examples = [new (null, $"{Keyword} l {EID.Examples[0]}")]},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new AccountSitesRequest(AccountIdentifier.Parse(Args[0].Name)));

												Dump(rp.Sites.Select(i => Rdc(new SiteRequest(i)).Site), ["Id", "Title", "Team", "Cards"], [i => i.Id, i => i.Title, i => i.Owners.Length, i => i.Roots?.Length]);
					
												return rp.Sites;
											}
						},

					];	
	}
}
