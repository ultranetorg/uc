using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class ProductCommand : FairCommand
{
	new AutoId		First => AutoId.Parse(base.First);
	Argument		Eligible => ByArgument("Name of the user eligible to change the product");

	public ProductCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string type = nameof(type);

		a.Name = "c";
		a.Description = "Creates a product entity under the specified author";
		a.Arguments =  [new (null, EID, "Author Id to create the product under", Flag.First),
						new (type, PRODUCTTYPE, "A type of product"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ProductCreation {Type = GetEnum<ProductType>(type), Author = AutoId.Parse(Args[0].Name)};
							};
		return a;
	}
		
	public CommandAction Destroy()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Description = "Destroys existing product and all its associated data";
		a.Arguments =  [new (null, EID, "Id of the product to delete", Flag.First),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ProductDeletion {Product = First};
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string definition = nameof(definition);

		a.Name = "u";
		a.Description = "Updates a product properties";
		a.Arguments =  [new (null, EID, "Id of a product to update", Flag.First),
						new (definition, TEXT, "Product definition"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

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
		a.Description = "Gets information about product specified";
		a.Arguments =  [new (null, EID, "Id of a product to get information about", Flag.First), 
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var	r = Ppc(new ProductPpc(First)).Product;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
