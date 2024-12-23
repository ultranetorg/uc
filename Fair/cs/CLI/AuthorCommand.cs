namespace Uccs.Fair;

public class AuthorCommand : FairCommand
{
	public const string Keyword = "author";

	EntityId FirstAuthorId => EntityId.Parse(Args[0].Name);

	public AuthorCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{
		Actions =	[
						new ()
						{
							Names = ["c", "create"],

							Help = new Help()
							{
								Title = "Create",
								Description = "Creates a new author for a specified period",
								Syntax = $"{Keyword} c|acquire years=YEARS signer=UAA",

								Arguments =
								[
									new ("years", "Integer number of years in [1..10] range"),
									new ("signer", "Address of account that owns or is going to register the author")
								],

								Examples =
								[
									new (null, $"{Keyword} c years=5 signer=0x0000fffb3f90771533b1739480987cee9f08d754")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												return new AuthorCreation {Years = byte.Parse(GetString("years"))};
											}
						},

						new ()
						{
							Names = ["e", "entity"],

							Help = new Help()
							{
								Title = "Entity",
								Description = "Get author entity information from MCV database",
								Syntax = $"{Keyword} e|entity AUTD",

								Arguments =
								[
									new ("<first>", "Id of an author to get information about")
								],

								Examples =
								[
									new (null, $"{Keyword} e 12345-678")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcQueryTimeout);
				
												var rp = Rdc(new AuthorRequest(FirstAuthorId));

												Dump(rp.Author);
					
												return rp.Author;
											}
						},

						new ()
						{
							Names = ["r", "renew"],

							Help = new Help()
							{
								Title = "Renew",
								Description = "Extend author rent for a specified period. Allowed during the last year of current period only.",
								Syntax = $"{Keyword} r|renew AUID years=YEARS signer=UAA",

								Arguments =
								[
									new ("<first>", "Id of an author to be renewed"),
									new ("years", "Integer number of years in [1..10] range"),
									new ("signer", "Address of account that owns the author")
								],

								Examples =
								[
									new (null, $"{Keyword} r 12345-678 years=5 signer=0x0000fffb3f90771533b1739480987cee9f08d754")
								]
							},

							Execute = () =>	{
												Flow.CancelAfter(program.Settings.RdcTransactingTimeout);

												var d = Rdc(new AuthorRequest(FirstAuthorId)).Author;

												return new AuthorUpdation  {Action	= AuthorAction.Renew,
																			Id		= d.Id,
																			Years	= byte.Parse(GetString("years"))};
											}
						},

					];	
	}
}
