namespace Uccs.Fair;

public class PageCommand : FairCommand
{
	public const string Keyword = "page";

	EntityId FirstPageId => EntityId.Parse(Args[0].Name);

	public PageCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help()
							{
								Title = "Create",
								Description = "Creates a new page",
								Syntax = $"{Keyword} c|create site={EID} product={EID} {SignerArg}={AA}",
								Arguments =	[new ("site", "Number of years in [1..10] range"),
											 new ("product", "Number of years in [1..10] range"),
											 new (SignerArg, "Address of account that owns the site")],
								Examples =	[new (null, $"{Keyword} c site={EID.Examples[0]} product={EID.Examples[1]} {SignerArg}=0x0000fffb3f90771533b1739480987cee9f08d754")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var o = new PageCreation();

												o.Site = GetEntityId("site"); 

												if(Has("permissions"))
												{	
													o.Flags |= PageFlags.Permissions;
													o.Permissions = PagePermissions.Parse(GetString("permissions")); 
												}
												
												if(One("content") is var c)
												{
													o.Flags |= PageFlags.Content;
													o.Content = new (c.GetEnum<PageType>("type"),
																	 c.GetEnum<PageType>("type") switch
																								 {
																								 	PageType.Content => c.Has("plain") ? c.Get<string>("plain") : File.ReadAllText(c.Get<string>("file")),
																								 	PageType.Product => new ProductData(EntityId.Parse(c.Get<string>("product")), c.Get<string>("sections").Split(',')),
																								 	_ => throw new SyntaxException("Unknown content type")
																								 });
												}
												return o;
											}
						},

						new ()
						{
							Names = ["x", "destroy"],

							Help = new Help
							{ 
								Title = "Destroy",
								Description = "Destroys existing page and all its associated data",
								Syntax = $"{Keyword} x|destroy {EID}",
								Arguments = [new ("<first>", "Id of a page to delete")],
								Examples = [new (null, $"{Keyword} x {EID.Examples[0]}")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new PageDeletion {Page = FirstPageId};
											}
						},
						new ()
						{
							Names = ["e", "entity"],

							Help = new Help()
							{
								Title = "Entity",
								Description = "Get page entity information from MCV database",
								Syntax = $"{Keyword} e|entity {EID}",
								Arguments =	[new ("<first>", "Id of a page to get information about")],
								Examples =[new (null, $"{Keyword} e {EID.Examples[0]}")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new PageRequest(FirstPageId));

												Dump(rp.Page);
					
												return rp.Page;
											}
						},

						new ()
						{
							Names = ["l", "list"],

							Help = new Help {Title = "List",
											 Description = "Get pages of a specified site",
											 Syntax = $"{Keyword} l|list {EID}",
											 Arguments = [new ("<first>", "Id of a catalog to get pages from")],
											 Examples = [new (null, $"{Keyword} l {EID.Examples[0]}")]},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new SitePagesRequest(FirstPageId));

												Dump(rp.Pages.Select(i => Rdc(new PageRequest(i)).Page), ["Id", "Site", "Content", "Pages", "Comments"], [i => i.Id, i => i.Site, i => i.Content.Type, i => i.Pages?.Length, i => i.Comments?.Length]);
					
												return rp.Pages;
											}
						},

					];	
	}
}
