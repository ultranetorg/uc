using System.Reflection;

namespace Uccs.Fair;

public class AuthorCommand : FairCommand
{
	EntityId FirstAuthorId => EntityId.Parse(Args[0].Name);

	public AuthorCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Help = new() {Description = "Creates a new author for a specified period",
						Syntax = $"{Keyword} {a.NamesSyntax} years={INT} {SignerArg}={AA}",
						
						Arguments =	[new ("years", "Number of years in [1..10] range"),
									 new (SignerArg, "Address of account that owns or is going to register the author")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} years=5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorCreation {Years = byte.Parse(GetString("years"))};
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Help = new() {Description = "Get author entity information from MCV database",
						Syntax = $"{Keyword} {a.NamesSyntax}{EID}",
						
						Arguments =	[new ("<first>", "Id of an author to get information about")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new AuthorRequest(FirstAuthorId));

								Dump(rp.Author);
					
								return rp.Author;
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "r";
		a.Help = new() {Description = "Extend author rent for a specified period. Allowed during the last year of current period only.",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} years={INT} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Id of an author to be renewed"),
										new ("years", "Integer number of years in [1..10] range"),
										new (SignerArg, "Address of account that owns the author")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {EID.Example} years=5 {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Rdc(new AuthorRequest(FirstAuthorId)).Author;

								return new AuthorUpdation  {Action	= AuthorAction.Renew,
															Id		= d.Id,
															Years	= byte.Parse(GetString("years"))};
							};
		return a;
	}

	public CommandAction List()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());

		a.Name = "l";
		a.Help = new() {Description = "Get authors that specified account owns",
						Syntax = $"{Keyword} {a.NamesSyntax} {AAID}",
						
						Arguments = [new ("<first>", "Id of an account to get authors from")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example}"),
									 new (null, $"{Keyword} {a.Name} {AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Rdc(new AccountAuthorsRequest(FirstAuthorId));

								Dump(rp.Authors, ["Id"], [i => i]);
					
								return rp.Authors;
							};
		return a;
	}
}
