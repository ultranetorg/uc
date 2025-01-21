using System.Reflection;

namespace Uccs.Smp;

public class PublicationCommand : SmpCommand
{
	EntityId FirstEntityId => EntityId.Parse(Args[0].Name);

	public PublicationCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		
	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		const string p = "product";
		const string c = "category";

		a.Name = "c";
		a.Help = new() {Description = "Creates a new product publication",
						Syntax = $"{Keyword} {a.NamesSyntax} {p}={EID} {c}={EID} {SignerArg}={AA}",

						Arguments =	[new (p, "An id of a product to add a publication of"),
									 new (c, "An id of category to publicate product under"),
									 new (SignerArg, "Address of account with corresponding permissions")],

						Examples =	[new (null, $"{Keyword} {a.Name} {p}={EID.Example} {c}={EID.Examples[1]} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new PublicationCreation() {Product = GetEntityId(p), Category = GetEntityId(c)};
								
								return o;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		const string p = "product";
		const string s = "sections";

		a.Name = "u";
		a.Help = new() {Description = "Updates data of speciofied publication",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} [{p}={EID}] [{s}={NAME},{NAME}...{NAME}] {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of publication to update"),
									 new (p, "An id of product id"),
									 new (s, "List of comma-separated sections names"),
									 new (SignerArg, "Address of account that assumed to have permissions to make changes specified")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {p}={EID.Examples[1]} {SignerArg}={AA.Example}"),
									 new (null, $"{Keyword} {a.Name} {EID.Example} {s}={ProductProperty.Description} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new PublicationUpdation();

								o.Publication = FirstEntityId;

								if(One(p)?.Value is EntityId e)
								{
									o.Change = PublicationChange.Product;
									o.Value	=  e;
								}
								else if(One(s)?.Value is string v)
								{
									o.Change = PublicationChange.Sections;
									o.Value =  v.Split(',');
								}
								else
									throw new SyntaxException("Unknown arguments");
								
								return o;
							};
		return a;
	}

	public CommandAction Delete()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "x";
		a.Help = new() {Description = "Destroys existing publication and all its associated data",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments = [new ("<first>", "Id of a publication to delete")],
						Examples = [new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new PublicationDeletion {Publication = FirstEntityId};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get publication entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID}",
						Arguments =	[new ("<first>", "Id of a publication to get information about")],
						Examples =[new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new PublicationRequest(FirstEntityId));

								Dump(rp.Publication);
					
								return rp.Publication;
							};
		return a;
	}

}
