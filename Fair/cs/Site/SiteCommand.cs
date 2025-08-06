using System.Reflection;

namespace Uccs.Fair;

public class SiteCommand : FairCommand
{
	public SiteCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a new site",
						Syntax = $"{Keyword} {a.NamesSyntax} years={INT} title={NAME} {SignerArg}={AA}",
						
						Arguments =	[new ("years", "Integer number of years in [1..10] range"),
									 new ("title", "A title of a site being created"),
									 new (SignerArg, "Address of account that owns or is going to register the site")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} years=5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new SiteCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get site entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments =	[new (FirstArg, "Id of an site to get information about")],
						Examples =[new (null, $"{Keyword} e {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new SiteRequest(FirstEntityId));

								Flow.Log.Dump(rp.Site);
					
								return rp.Site;
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var years = "years";

		a.Name = "r";
		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {years}={INT} {SignerArg}={AA}",

						Arguments =	[new (FirstArg, "Id of a site to update"),
									 new (years, "A number of years to renew site for. Allowed during the last year of current period only."),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {years}=5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new SiteRenewal {SiteId = FirstEntityId, Years = byte.Parse(GetString(years))};
							};
		return a;
	}

	public CommandAction Nickname()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var nickname = "nickname";

		a.Name = "n";
		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {EID} {As}={ROLE} {nickname}={NAME} {SignerArg}={AA}",

						Arguments =	[new (FirstArg, "Id of a site to update"),
									 new (nickname, "A new nickname"),
									 new (As, "On behalf of"),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {EID.Example1} {nickname}={NAME.Example} {As}={Role.Moderator} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProposalCreation(FirstEntityId, SecondEntityId, GetEnum<Role>(@As), new SiteNicknameChange {Nickname = GetString(nickname)}); 
							};
		return a;
	}

	public CommandAction ListCategories()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "lc";
		a.Help = new() {Description = "Get categories of a specified site",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new (FirstArg, "Id of a site to get categories from")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new SiteCategoriesRequest(FirstEntityId));

								Flow.Log.DumpFixed(rp.Categories.Select(i => Ppc(new CategoryRequest(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}

	public CommandAction Property()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var t = "title";
		var s = "slogan";
		var d = "description";

		a.Name = "p";
		a.Help = new() {Description = "Changes various site's descriptive properties",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {EID} {As}={ROLE} {t}={TEXT} {s}={TEXT} {d}={TEXT} {SignerArg}={AA}",

						Arguments =	[new (FirstArg, "Id of a site to update"),
									 new ("<second>", "Id of a actor"),
									 new (@As, $"A role of actor, {Uccs.Fair.Role.Moderator} by default"),
									 new (t, "A new title"),
									 new (s, "A new slogan"),
									 new (d, "A new description"),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {EID.Example1} {@As}={ROLE.Example1} {s}={TEXT.Example} {SignerArg}={AA.Example}")]};

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
