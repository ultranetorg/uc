using System.Reflection;

namespace Uccs.Fair;

public class SiteCommand : FairCommand
{
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
						ByArgument("Address of account that owns or is going to register the site")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

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
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new SitePpc(FirstEntityId));

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
							 ByArgument("Address of account that owns the site")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new SiteRenewal {SiteId = FirstEntityId, Years = byte.Parse(GetString(years))};
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
						ByArgument("Address of account that owns the site")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProposalCreation(FirstEntityId, SecondEntityId, GetEnum<Role>(@As), new SiteNicknameChange {Nickname = GetString(nickname)}); 
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
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new SiteCategoriesPpc(FirstEntityId));

								Flow.Log.DumpFixed(rp.Categories.Select(i => Ppc(new CategoryPpc(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}

	public CommandAction Property()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var t = "title";
		var s = "slogan";
		var d = "description";

		a.Name = "p";
		a.Description = "Changes various site's descriptive properties";
		a.Arguments =	[new (null, EID, "Id of a site to update", Flag.First),
						new (null, EID, "Id of a actor", Flag.Second),
						new (@As, ROLE, $"A role of actor, {Uccs.Fair.Role.Moderator} by default"),
						new (t, TEXT, "A new title", Flag.Optional),
						new (s, TEXT, "A new slogan", Flag.Optional),
						new (d, TEXT, "A new description", Flag.Optional),
						ByArgument("Address of account that owns the site")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new SiteTextChange();

								o.Title			= GetString(t, null); 
								o.Slogan		= GetString(s, null); 
								o.Description	= GetString(d, null); 

								return new ProposalCreation(FirstEntityId, SecondEntityId, GetEnum<Role>(@As, Uccs.Fair.Role.Moderator), o);
							};
		return a;
	}
}
