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
		
		var y = "years";
		var dp = "deposit";
		var mr = "mr";
		var ow = "owner";

		a.Name = "u";
		a.Help = new() {Description = "Extend author rent for a specified period. Allowed during the last year of current period only.",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {y}={INT} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Id of an author to be renewed"),
										new (y, "A number of years to renew author for"),
										new (SignerArg, "Address of account that owns the author")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {EID.Example} {y}=5 {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var d = Ppc(new AuthorRequest(FirstAuthorId)).Author;

								var o = new AuthorUpdation {Id = d.Id};

								if(Has(y))
								{
									o.Change = AuthorChange.Renew;
									o.Value	 = byte.Parse(GetString(y));
								}
								else if(Has(dp))
								{
									o.Change = AuthorChange.Deposit;
									o.Value	 = GetLong(dp);
								}
								else if(Has(mr))
								{
									o.Change = AuthorChange.Deposit;
									o.Value	 = GetInt(mr);
								}
								else if(Has(ow))
								{
									o.Change = AuthorChange.Owner;
									o.Value	 = GetAccountAddress(ow);
								}

								return o;
							};
		return a;
	}
}
