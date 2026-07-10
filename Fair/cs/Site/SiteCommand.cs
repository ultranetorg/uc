using System.Reflection;

namespace Uccs.Fair;

public class SiteCommand : FairCommand
{
	Argument Eligible => ByArgument("A name of user eligible to propose changes to the site");

	public SiteCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Creates a new site";
		a.Arguments =  [new ("years", INT, "Integer number of years in [1..10] range"),
						new ("title", NAME, "A title of a site being created"),
						ByArgument("A name of suer that is going to register the site")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new SiteCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get site entity information from MCV database";
		a.Arguments =	[new (null, EID, "Id of an site to get information about", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new SitePpc(FirstAutoId));

								Flow.Log.Dump(rp.Site);
					
								return rp.Site;
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var years = "years";

		a.Name = "r";
		a.Description = "Prolongs current expiration date of a site for a specified number of years";
		a.Arguments =  [new (null, EID, "Id of a site to update", Flag.First),
							 new (years, INT, "A number of years to renew site for. Allowed during the last year of current period only."),
							 Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new SiteRenewal {SiteId = FirstAutoId, Years = byte.Parse(GetString(years))};
							};
		return a;
	}

	public CommandAction Nickname()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var nickname = "nickname";

		a.Name = "n";
		a.Description = "Sets a nickname for a specified site";
		a.Arguments =  [new (null, EID, "Id of a site to update", Flag.First),
						new (null, EID, "Id of a creator", Flag.Second),
						new (nickname, EID, "A new nickname"),
						new (As, ROLE, "On behalf of"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ProposalCreation(FirstAutoId, SecondAutoId, GetEnum<Role>(@As), new SiteNameChange {Name = GetString(nickname)}); 
							};
		return a;
	}

	public CommandAction ListCategories()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "lc";
		a.Description = "Get categories of a specified site";
		a.Arguments = [new (null, EID, "Id of a site to get categories from", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new SiteCategoriesPpc(FirstAutoId));

								Flow.Log.DumpFixed(rp.Categories.Select(i => Ppc(new CategoryPpc(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}

	public CommandAction Info()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var t = "title";
		var s = "slogan";
		var d = "description";

		a.Name = "i";
		a.Description = "Changes information abourt a site";
		a.Arguments =  [new (null, EID, "Id of a site to update", Flag.First),
						new (@As, ROLE, $"A role of actor, {Uccs.Fair.Role.Moderator} by default"),
						new (null, EID, "Id of a actor", Flag.Second),
						new (t, TEXT, "A new title", Flag.Optional),
						new (s, TEXT, "A new slogan", Flag.Optional),
						new (d, TEXT, "A new description", Flag.Optional),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var o = new SiteTextChange();

								o.Title			= GetString(t, null); 
								o.Slogan		= GetString(s, null); 
								o.Description	= GetString(d, null); 

								return new ProposalCreation(FirstAutoId, SecondAutoId, GetEnum<Role>(@As, Uccs.Fair.Role.Moderator), o);
							};
		return a;
	}
}
