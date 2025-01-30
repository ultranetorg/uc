using System.Reflection;

namespace Uccs.Smp;

/// <summary>
/// Usage: 
///		release publish 
/// </summary>
public class ProductCommand : SmpCommand
{
	EntityId First => EntityId.Parse(Args[0].Name);

	public ProductCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
	}

	public CommandAction Crete()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a product entity in the MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {SignerArg}={AA}",

						Arguments = [new ("data", "A data associated with the product")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProductCreation(EntityId.Parse(Args[0].Name));
							};
		return a;
	}

	public CommandAction Destroy()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Help = new() {Description = "Destroys existing product and all its associated data",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {SignerArg}={AA}",

						Arguments = [new ("<first>", "Id of a product to delete")],

						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new ProductDeletion {Product = First};
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "u";
		a.Help = new() {Description = "Updates a product entity properties in the MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} [description={TEXT}] {SignerArg}={AA}",

						Arguments = [new ("<first>", "Address of a product to update"),
									 new ("description", "A description text of a product")],

						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var	r = Ppc(new ProductRequest(First)).Product;

								var o =	new ProductUpdation(r.Id);

								if(Has("description"))
									o.Change(ProductField.Description, GetString("description"));

								return o;
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";

		a.Help = new( ){Description = "Gets product entity information from the MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",

						Arguments = [new ("<first>", "Id of a product to get information about")],

						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);

								var	r = Ppc(new ProductRequest(First)).Product;
				
								Dump(r);

								return r;
							};
		return a;
	}
}
