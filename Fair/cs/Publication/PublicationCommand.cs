using System.Reflection;

namespace Uccs.Fair;

public class PublicationCommand : FairCommand
{
	public PublicationCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string product = "product";
		const string site = "site";

		a.Name = "c";
		a.Description = "Creates a new product publication";
		a.Arguments =  [new (product, EID, "Id of a product to create publication for", Flag.First),
						new (site, EID, "Id of a site to create publication at"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var	p = Ppc(new ProductPpc(GetAutoId(product))).Product;
								var	a = Ppc(new AuthorPpc(p.Author)).Author;

								var o = new ProposalCreation
										{
											As = a.Sites.Contains(GetAutoId(site)) ? Role.Publisher : Role.Candidate, 
											Site = GetAutoId(site), 
											Options = [new Option(new PublicationCreation(GetAutoId(product)))]
										};
								
								return o;
							};
		return a;
	}

	public CommandAction Permission()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string publish = "publish";

		a.Name = "p";
		a.Description = "Approve or revoke permission for a site to publish a publication";
		a.Arguments =  [new (null,		EID, "Id of a publication to manage", Flag.First),
						new (publish,	NAME, "Approve or revoke a permission to publish (approve/revoke)"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var o = new PublicationAuthorApproval
										{
											Publication = AutoId.Parse(Args[0].Name),
											Approved = GetString(publish) == "approve" ? true : (GetString(publish) == "revoke" ? false : throw new SyntaxException($"Unknown '{publish}' value"))
										};
								
								return o;
							};
		return a;
	}
// 
// 	public CommandAction Update()
// 	{
// 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
// 
// 		const string product = "product";
// 		const string approve = "approve";
// 		const string reject = "reject";
// 
// 		a.Name = "u";
// 		a.Description = "Updates data of speciofied publication",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID} [{product}={EID}] [{approve}={NAME} id={INT}] [{reject}={NAME} id={INT}] {SignerArg}={AA}",
// 
// 						Arguments =	[new (FirstArg, "Id of publication to update"),
// 									 new (product, "A new  product id"),
// 									 new (approve, "Approve a field change"),
// 									 new (reject, "Reject a field change"),
// 									 new (SignerArg, "An address of account that assumed to have permissions to make specified changes")],
// 
// 						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {product}={EID.Examples[1]} {SignerArg}={AA.Example}"),
// 									 new (null, $"{Keyword} {a.Name} {EID.Example} {approve}={ProductFieldName.Title} {SignerArg}={AA.Example}"),
// 									 new (null, $"{Keyword} {a.Name} {EID.Example} {reject}={ProductFieldName.Title} {SignerArg}={AA.Example}")
// 									 ]};
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
// 
// 								var o = new PublicationUpdation();
// 
// 								o.Publication = FirstEntityId;
// 
// 								if(One(product)?.Value is EntityId e)
// 								{
// 									o.Change = PublicationChange.Product;
// 									o.Value	=  e;
// 								}
// 								else if(One(approve)?.Value is string av)
// 								{
// 									o.Change = PublicationChange.ApproveChange;
// 									o.Value = new ProductFieldVersionReference {Name = av, Version = GetInt("id")};
// 								}
// 								else if(One(reject)?.Value is string rv)
// 								{
// 									o.Change = PublicationChange.RejectChange;
// 									o.Value = new ProductFieldVersionReference {Name = rv, Version = GetInt("id")};
// 								}
// 								else
// 									throw new SyntaxException("Unknown arguments");
// 								
// 								return o;
// 							};
// 		return a;
// 	}
// 
// 	public CommandAction Delete()
// 	{
// 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
// 
// 		a.Name = "x";
// 		a.Description = "Destroys existing publication and all its associated data",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						a.Arguments = [new (FirstArg, "Id of a publication to delete")],
// 						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")];
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
// 
// 								return new PublicationDeletion {Publication = FirstEntityId};
// 							};
// 		return a;
// 	}
// 
// 	public CommandAction Entity()
// 	{
// 		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
// 
// 		a.Name = "e";
// 		a.Description = "Get publication entity information from MCV database",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						Arguments =	[new (FirstArg, "Id of a publication to get information about")],
// 						Examples =[new (null, $"{Keyword} {a.Name} {EID.Example}")];
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
// 				
// 								var rp = Ppc(new PublicationRequest(FirstEntityId));
// 
// 								Dump(rp.Publication);
// 					
// 								return rp.Publication;
// 							};
// 		return a;
// 	}
}
