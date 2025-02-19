using System.Reflection;

namespace Uccs.Fair;

public class CategoryCommand : FairCommand
{
	EntityId FirstEntityId => EntityId.Parse(Args[0].Name);

	public CategoryCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a new category",
						Syntax = $"{Keyword} {a.NamesSyntax} [site={EID}] [parent={EID}] title={NAME} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Page type"),
									 new ("site", "An id of site to add a category to"),
									 new ("parent", "An id of parent category to add a new category to"),
									 new ("title", "A title of a category being created"),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} site={EID.Example} title={NAME.Example} {SignerArg}={AA.Example}"),
									 new (null, $"{Keyword} {a.Name} parent={EID.Example} title={NAME.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new CategoryCreation() {Site = GetEntityId("site", null), 
																Parent = GetEntityId("parent", null), 
																Title = GetString("title")};
								
								return o;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";
		a.Help = new() {Description = "Updates data of speciofied category",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} [parent={EID}] {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of category to update"),
									 new ("parent", "An id of a new parent"),
									 new (SignerArg, "Address of account that assumed to have permissions to make specified changes")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} parent={EID.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new CategoryUpdation();

								o.Category = FirstEntityId;

								if(One("parent")?.Value is string id)
								{	
									o.Change = CategoryChange.Move;
									o.Value = EntityId.Parse(id); 
								}

								return o;
							};
		return a;
	}

	public CommandAction ListPublications()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "lp";
		a.Help = new() {Description = "Get publications of a specified category",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a category to get publications of")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new CategoryPublicationsRequest(FirstEntityId));

								DumpFixed(rp.Publications.Select(i => Ppc(new PublicationRequest(i)).Publication).ToArray(), ["Id", "Product", "Category"], [i => i.Id, i => i.Product, i => i.Category]);
					
								return rp.Publications;
							};
		return a;
	}

	public CommandAction ListCategories()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "lc";
		a.Help = new() {Description = "Get subcategories of a specified category",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a site to get subcategories from")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new CategoryCategoriesRequest(FirstEntityId));

								DumpFixed(rp.Categories.Select(i => Ppc(new CategoryRequest(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}

// 	public CommandAction Delete()
// 	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		a.Name = "x";
// 		a.Help = new() {Description = "Destroys existing category and all its associated data",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						Arguments = [new ("<first>", "Id of a category to delete")],
// 						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
// 
// 								return new CategoryDeletion {Category = FirstPageId};
// 							};
// 		return a;
// 	}
// 
// 	public CommandAction Entity()
// 	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		a.Name = "e";
// 		a.Help = new() {Description = "Get category entity information from MCV database",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						Arguments =	[new ("<first>", "Id of a category to get information about")],
// 						Examples =[new (null, $"{Keyword} {a.Name} {EID.Example}")]};
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
// 				
// 								var rp = Rdc(new PageRequest(FirstPageId));
// 
// 								Dump(rp.Page);
// 					
// 								return rp.Page;
// 							};
// 		return a;
// 	}
// 
// 	public CommandAction List()
// 	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		a.Name = "l";
// 		a.Help = new() {Description = "Get categorys of a specified site",
// 						Syntax = $"{Keyword}  {a.NamesSyntax} {EID}",
// 						Arguments = [new ("<first>", "Id of a catalog to get categorys from")],
// 						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
// 				
// 								var rp = Rdc(new SitePagesRequest(FirstPageId));
// 
// 								Dump(rp.Pages.Select(i => Rdc(new PageRequest(i)).Page), ["Id", "Site", "Data", "Pages", "Comments"], [i => i.Id, i => i.Site, i => i.Data, i => i.Pages?.Length, i => i.Comments?.Length]);
// 					
// 								return rp.Pages;
// 							};
// 		return a;
// 	}
}
