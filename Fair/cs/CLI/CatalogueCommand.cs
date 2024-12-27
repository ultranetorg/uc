namespace Uccs.Fair;

public class CatalogueCommand : FairCommand
{
	public const string Keyword = "catalogue";

	EntityId FirstCatalogueId => EntityId.Parse(Args[0].Name);

	public CatalogueCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help()
							{
								Title = "Create",
								Description = "Creates a new catalogue",
								Syntax = $"{Keyword} c|create title=TEXT signer=UAA",
								Arguments =	[new ("years", "Integer number of years in [1..10] range"),
											 new (SignerArg, "Address of account that owns or is going to register the catalogue")],
								Examples =	[new (null, $"{Keyword} c title=\"The Store\" signer=0x0000fffb3f90771533b1739480987cee9f08d754")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new CatalogueCreation {Title = GetString("title")};
											}
						},

						new ()
						{
							Names = ["e", "entity"],

							Help = new Help()
							{
								Title = "Entity",
								Description = "Get catalogue entity information from MCV database",
								Syntax = $"{Keyword} e|entity AUID",
								Arguments =	[new ("<first>", "Id of an catalogue to get information about")],
								Examples =[new (null, $"{Keyword} e 12345-678")]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new CatalogueRequest(FirstCatalogueId));

												Dump(rp.Catalogue);
					
												return rp.Catalogue;
											}
						},

						new ()
						{
							Names = ["l", "list"],

							Help = new Help {Title = "List",
											 Description = "Get catalogues of a specified account",
											 Syntax = $"{Keyword} l|list AID",
											 Arguments = [new ("<first>", "Id of an account to get catalogues from")],
											 Examples = [new (null, $"{Keyword} l 12345-678")]},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new AccountCataloguesRequest(FirstCatalogueId));

												Dump(rp.Catalogues.Select(i => Rdc(new CatalogueRequest(i)).Catalogue), ["Id", "Title", "Team", "Cards"], [i => i.Id, i => i.Title, i => i.Owners.Length, i => i.Topics?.Length]);
					
												return rp.Catalogues;
											}
						},

					];	
	}
}
