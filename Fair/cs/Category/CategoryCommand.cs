using System.Reflection;

namespace Uccs.Fair;

public class CategoryCommand : FairCommand
{
	public CategoryCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
	}

//	public CommandAction Create()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//
//		a.Name = "c";
//		a.Description = "Creates a new category",
//						Syntax = $"{Keyword} {a.NamesSyntax} [site={EID}] [parent={EID}] title={NAME} {SignerArg}={AA}",
//
//						Arguments =	[new (FirstArg, "Page type"),
//									 new ("site", "An id of site to add a category to"),
//									 new ("parent", "An id of parent category to add a new category to"),
//									 new ("title", "A title of a category being created"),
//									 new (SignerArg, "Address of account that owns the site")],
//
//						Examples =	[new (null, $"{Keyword} {a.Name} site={EID.Example} title={NAME.Example} {SignerArg}={AA.Example}"),
//									 new (null, $"{Keyword} {a.Name} parent={EID.Example} title={NAME.Example} {SignerArg}={AA.Example}")];
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
//
//								var o = new ProposalCreation 
//										{
//											Site = GetEntityId("site", null), 
//											Options = [new Option  (new CategoryCreation()
//																	{ 
//																		Parent = GetEntityId("parent", null), 
//																		Title = GetString("title")
//																	})]
//										};
//								
//								return o;
//							};
//		return a;
//	}

	//public CommandAction Update()
	//{
	//	var a = new CommandAction(this, MethodBase.GetCurrentMethod());
	//
	//	a.Name = "u";
	//	a.Description = "Updates data of specified category",
	//					Syntax = $"{Keyword} {a.NamesSyntax} {EID} [parent={EID}] {SignerArg}={AA}",
	//
	//					Arguments =	[new (FirstArg, "Id of category to update"),
	//								 new ("parent", "An id of a new parent"),
	//								 new ("type", "Type"),
	//								 new (SignerArg, "Address of account that assumed to have permissions to make specified changes")],
	//
	//					Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} parent={EID.Example} {SignerArg}={AA.Example}")];
	//
	//	a.Execute = () =>	{
	//							Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
	//
	//							var o = new ProposalCreation();
	//
	//							o.Site = Ppc(new CategoryRequest()).Category.Site;
	//
	//							if(One("parent")?.Value is string id)
	//							{	
	//								o.Option = new CategoryMovement {Category = FirstEntityId, Parent = AutoId.Parse(id)};
	//								return o;
	//							}
	//
	//							throw new SynchronizationException("Wrong arguments");
	//						};
	//	return a;
	//}

	public CommandAction ListPublications()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "lp";
		a.Description = "Get publications of a specified category";
		a.Arguments = [new (null, EID, "Id of a category to get publications of", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new CategoryPublicationsRequest(FirstEntityId));

								Flow.Log.DumpFixed(rp.Publications.Select(i => Ppc(new PublicationRequest(i)).Publication), ["Id", "Product", "Category"], [i => i.Id, i => i.Product, i => i.Category]);
					
								return rp.Publications;
							};
		return a;
	}

	public CommandAction ListCategories()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "lc";
		a.Description = "Get subcategories of a specified category";
		a.Arguments = [new (null, EID, "Id of a site to get subcategories from", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new CategoryCategoriesRequest(FirstEntityId));

								Flow.Log.DumpFixed(rp.Categories.Select(i => Ppc(new CategoryRequest(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
					
								return rp.Categories;
							};
		return a;
	}
}
