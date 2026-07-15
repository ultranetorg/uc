using System.Reflection;
using System.Text;

namespace Uccs.Fair;

public class ProductCommand : FairCommand
{
	Argument		Eligible => ByArgument("Name of the user eligible to change the product");

	public ProductCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string author = nameof(author);
		const string type = nameof(type);

		a.Description = "Creates a product entity under the specified author";
		a.Arguments =	[
							new (author, EID, "Author Id to create the product under", ArgumentFlag.First),
							new (type, PRODUCTTYPE, "A type of product"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ProductCreation {Author = AutoId.Parse(GetString(author)), Type = GetEnum<ProductType>(type)};
							};
		return a;
	}
		
	public CommandAction Destroy_X()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Destroys existing product and all its associated data";
		a.Arguments =	[
							IdArgument("product to delete"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new ProductDeletion {Product = Id};
							};
		return a;
	}

	public CommandAction Update_U()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string definition = nameof(definition);

		a.Description = "Updates a product properties";
		a.Arguments =	[
							IdArgument("product to update"),
							new (definition, TEXT, "Product definition"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var	r = Ppc(new ProductPpc(Id)).Product;

								var o =	new ProductUpdation(Id);

								o.Fields = Product.ParseDefinition(Product.FindDeclaration(r.Type), GetString(definition));

								return o;
							};
		return a;
	}

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Gets information about product specified";
		a.Arguments =	[
							IdArgument("product to get information about"), 
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);

								var	r = Ppc(new ProductPpc(Id)).Product;
				
								Flow.Log.Dump(r);

								return r;
							};
		return a;
	}
}
