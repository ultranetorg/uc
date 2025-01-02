using System.Reflection;

namespace Uccs.Fair;

public class PageCommand : FairCommand
{
	EntityId FirstPageId => EntityId.Parse(Args[0].Name);

	public PageCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";

		a.Help = new() {Description = "Creates a new page",
						Syntax = $"{Keyword} {a.NamesSyntax} site={EID} {SignerArg}={AA}",

						Arguments =	[new ("site", "Id of site to add page to"),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} site={EID.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new PageCreation();

								o.Site = GetEntityId("site"); 

								return o;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";
		a.Help = new() {Description = "Updates speciofied page",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} [permissions=PERMISSIONS] [pages={EID},{EID}...{EID}] [content {{type=content|product [plain={TEXT} | file={FILEPATH}] [product={EID} sections={NAME},{NAME}...{NAME}]}}] {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of page to update"),
									 new ("permissions", ""),
									 new ("pages", ""),
									 new ("content", ""),
									 new ("content/type", ""),
									 new ("content/plain", ""),
									 new ("content/path", ""),
									 new ("content/product", ""),
									 new ("content/sections", ""),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} site={EID.Example[1]} content={{type=Content plain={TEXT.Example}}} {SignerArg}={AA.Example}"),
									 new (null, $"{Keyword} {a.Name} site={EID.Example[1]} content={{type=Content file={FILEPATH.Example}}} {SignerArg}={AA.Example}"),
									 new (null, $"{Keyword} {a.Name} site={EID.Example[1]} content={{type=Product product={EID.Examples[1]}}} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new PageCreation();

								o.Site = GetEntityId("site"); 

	// 												if(Has("permissions"))
	// 												{	
	// 													o.Fields |= PageField.Permissions;
	// 													o.Permissions = PagePermissions.Parse(GetString("permissions")); 
	// 												}
	// 												
	// 												if(One("content") is var c)
	// 												{
	// 													o.Field |= PageField.Content;
	// 													o.Content = new (c.GetEnum<PageType>("type"),
	// 																	 c.GetEnum<PageType>("type") switch
	// 																								 {
	// 																								 	PageType.Content => c.Has("plain") ? c.Get<string>("plain") : File.ReadAllText(c.Get<string>("file")),
	// 																								 	PageType.Product => new ProductData(EntityId.Parse(c.Get<string>("product")), c.Get<string>("sections").Split(',')),
	// 																								 	_ => throw new SyntaxException("Unknown content type")
	// 																								 });
	// 												}
								return o;
							};
		return a;
	}

	public CommandAction Destroy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Help = new() {Description = "Destroys existing page and all its associated data",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a page to delete")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new PageDeletion {Page = FirstPageId};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get page entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments =	[new ("<first>", "Id of a page to get information about")],
						Examples =[new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new PageRequest(FirstPageId));

								Dump(rp.Page);
					
								return rp.Page;
							};
		return a;
	}

	public CommandAction List()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Get pages of a specified site",
						Syntax = $"{Keyword}  {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a catalog to get pages from")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new SitePagesRequest(FirstPageId));

								Dump(rp.Pages.Select(i => Rdc(new PageRequest(i)).Page), ["Id", "Site", "Content", "Pages", "Comments"], [i => i.Id, i => i.Site, i => i.Content.Type, i => i.Pages?.Length, i => i.Comments?.Length]);
					
								return rp.Pages;
							};
		return a;
	}
}
