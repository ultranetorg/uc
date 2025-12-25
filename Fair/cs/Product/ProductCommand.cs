using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class ProductCommand : FairCommand
{
	AutoId First => AutoId.Parse(Args[0].Name);

	public ProductCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		var type = "type";

		a.Name = "c";
		a.Description = "Creates a product entity in the MCV database";
		a.Arguments =  [new (null, EID, "Entity id of author to create a product for", Flag.First),
						new (type, PRODUCTTYPE, "A type of product"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProductCreation {Type = GetEnum<ProductType>(type), Author = AutoId.Parse(Args[0].Name)};
							};
		return a;
	}
		
	public CommandAction Destroy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Description = "Destroys existing product and all its associated data";
		a.Arguments =  [new (null, EID, "Id of a product to delete", Flag.First),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProductDeletion {Product = First};
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		string definition;	definition = nameof(definition);

		a.Name = "u";
		a.Description = "Updates a product entity properties in the MCV database";
		a.Arguments =  [new (null, EID, "Id of a product to update", Flag.First),
						new (definition, TEXT, "Product definition"),
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o =	new ProductUpdation(First);

								var	r = Ppc(new ProductPpc(First)).Product;

								if(Has(definition))
									o.Fields = Product.ParseDefinition(Product.FindDeclaration(r.Type), GetString(definition));

								return o;
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Gets product entity information from the MCV database";
		a.Arguments =  [new (null, EID, "Id of a product to get information about", Flag.First), 
						ByArgument()];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Ppc(new ProductPpc(First)).Product;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
