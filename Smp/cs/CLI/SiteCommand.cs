using System.Reflection;

namespace Uccs.Smp;

public class SiteCommand : SmpCommand
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
						Syntax = $"{Keyword} {a.NamesSyntax} {SITETYPE} years={INT} title={NAME} {SignerArg}={AA}",
						
						Arguments =	[new ("<first>", "A type of sire to create"),
									 new ("years", "Integer number of years in [1..10] range"),
									 new ("title", "A ttile of a site being created"),
									 new (SignerArg, "Address of account that owns or is going to register the site")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} {SiteType.Store} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new SiteCreation {Type = GetEnum<SiteType>(0), Title = GetString("title")};
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
				
								var rp = Ppc(new SiteRequest(FirstSiteId));

								Dump(rp.Site);
					
								return rp.Site;
							};
		return a;
	}

	public CommandAction ListCategories()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "lc";
		a.Help = new() {Description = "Get categories of a specified site",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a site to get categories from")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new SiteCategoriesRequest(FirstSiteId));

								Dump(rp.Categories.Select(i => Ppc(new CategoryRequest(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}

}
