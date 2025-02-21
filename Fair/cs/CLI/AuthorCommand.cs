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
						Syntax = $"{Keyword} {a.NamesSyntax} years={INT} title={NAME} {SignerArg}={AA}",
						
						Arguments =	[new ("years", "Number of years in [1..10] range"),
									 new ("title", "A ttile of a author being created"),
									 new (SignerArg, "Address of account that owns or is going to register the author")],
						
						Examples =	[new (null, $"{Keyword} {a.Name} years=5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
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
				
								var rp = Ppc(new AuthorRequest(FirstAuthorId));

								Dump(rp.Author);
					
								return rp.Author;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var owner = "owner";

		a.Name = "u";
		a.Help = new() {Description = "Extend author rent for a specified period. Allowed during the last year of current period only.",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {owner}={AA} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Id of an author to be renewed"),
										new (owner, "Account address of a new owner"),
										new (SignerArg, "Address of account that owns the author")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {EID.Example} {owner}={AA.Example1} {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new AuthorUpdation {AuthorId = FirstEntityId};
								
								if(Has(owner))
								{
									o.Change = AuthorChange.Owner;
									o.Value	 = GetAccountAddress(owner);
								}
								else
									throw new SyntaxException("Unknown parameters");

								return o;
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var years = "years";

		a.Name = "r";
		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {years}={INT} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of a author to update"),
									 new (years, "A number of years to renew author for. Allowed during the last year of current period only."),
									 new (SignerArg, "Address of account that owns the author")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {years}=5 {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new AuthorUpdation {AuthorId = FirstEntityId};

								o.Change = AuthorChange.Renew;
								o.Value	 = byte.Parse(GetString(years));

								return o;
							};
		return a;
	}
}
