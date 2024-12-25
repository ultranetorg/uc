namespace Uccs.Fair;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class ProductCommand : FairCommand
{
	public const string Keyword = "product";

	ProductId First => ProductId.Parse(Args[0].Name);

	public ProductCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help
							{ 
								Title = "Crete",
								Description = "Creates a product entity in the MCV database",
								Syntax = $"{Keyword} c|create AUID",

								Arguments = [new ("data", "A data associated with the product")],

								Examples =	[new (null, $"{Keyword} c 123456-78")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new ProductCreation(EntityId.Parse(Args[0].Name));
											}
						},

						new ()
						{
							Names = ["x", "destroy"],

							Help = new Help
							{ 
								Title = "DESTROY",
								Description = "Destroys existing product and all its associated data",
								Syntax = $"{Keyword} x|destroy PID",

								Arguments = [new ("<first>", "Id of a product to delete")],

								Examples = [new (null, $"{Keyword} x 12345-67-8")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var r = Rdc(new ProductRequest(First)).Product;

												return new ProductDeletion {Product = r.Id};
											}
						},

						new ()
						{
							Names = ["u", "update"],

							Help = new Help
							{ 
								Title = "Update",
								Description = "Updates a product entity properties in the MCV database",
								Syntax = $"{Keyword} u|update PID [data=PRODUCTDATA] description=[TEXT]",

								Arguments = [new ("<first>", "Address of a product to update"),
											 new ("description", "A description text of a product")],

								Examples = [new (null, $"{Keyword} u 123456-67-8")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var	r = Rdc(new ProductRequest(First)).Product;

												var o =	new ProductUpdation(r.Id);

												if(Has("description"))
													o.Change(ProductProperty.Description, GetString("description"));

												return o;
											}
						},

						new ()
						{
							Names = ["e", "entity"],

							Help = new Help
							{ 
								Title = "Entity",
								Description = "Gets product entity information from the MCV database",
								Syntax = $"{Keyword} e|entity PID",

								Arguments = [new ("<first>", "Id of a product to get information about")],

								Examples = [new (null, $"{Keyword} e 123456-67-8")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);

												var	r = Rdc(new ProductRequest(First)).Product;
				
												Dump(r);

												return r;
											}
						},
					];
	}

}
