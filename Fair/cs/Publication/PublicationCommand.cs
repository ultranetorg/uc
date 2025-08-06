using System.Reflection;

namespace Uccs.Fair;

// public class PublicationCommand : FairCommand
// {
// 	public PublicationCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
// 	{
// 		
// 	}
// 
// 	public CommandAction Create()
// 	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		const string p = "product";
// 		const string c = "category";
// 
// 		a.Name = "c";
// 		a.Help = new() {Description = "Creates a new product publication",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {p}={EID} {c}={EID} {SignerArg}={AA}",
// 
// 						Arguments =	[new (p, "An id of a product to add a publication of"),
// 									 new (c, "An id of category to publicate product under"),
// 									 new (SignerArg, "Address of account with corresponding permissions")],
// 
// 						Examples =	[new (null, $"{Keyword} {a.Name} {p}={EID.Example} {c}={EID.Examples[1]} {SignerArg}={AA.Example}")]};
// 
// 		a.Execute = () =>	{
// 								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);
// 
// 								var o = new PublicationCreation() {Product = GetEntityId(p), Category = GetEntityId(c)};
// 								
// 								return o;
// 							};
// 		return a;
// 	}
// 
// 	public CommandAction Update()
// 	{
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		const string product = "product";
// 		const string approve = "approve";
// 		const string reject = "reject";
// 
// 		a.Name = "u";
// 		a.Help = new() {Description = "Updates data of speciofied publication",
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
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		a.Name = "x";
// 		a.Help = new() {Description = "Destroys existing publication and all its associated data",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						Arguments = [new (FirstArg, "Id of a publication to delete")],
// 						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};
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
// 		var a = new CommandAction(MethodBase.GetCurrentMethod());
// 
// 		a.Name = "e";
// 		a.Help = new() {Description = "Get publication entity information from MCV database",
// 						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
// 						Arguments =	[new (FirstArg, "Id of a publication to get information about")],
// 						Examples =[new (null, $"{Keyword} {a.Name} {EID.Example}")]};
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
// 
// }
