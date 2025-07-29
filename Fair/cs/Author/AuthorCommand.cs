using System.Reflection;

namespace Uccs.Fair;

public class AuthorCommand : FairCommand
{
	AutoId FirstAuthorId => AutoId.Parse(Args[0].Name);

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
	
	public CommandAction Nickname()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var nickname = "nickname";

		a.Name = "n";
		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {nickname}={NAME} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of a author to update"),
									 new (nickname, "A new nickname"),
									 new (SignerArg, "Address of account that is author's owner")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {nickname}={NAME.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorNicknameChange{Author = FirstAuthorId,
																Nickname = GetString(nickname)}; 
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

								Flow.Log.Dump(rp.Author);
					
								return rp.Author;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var addowner = "addowner";
		var removeowner = "removeowner";

		a.Name = "u";
		a.Help = new() {Description = "Extend author rent for a specified period. Allowed during the last year of current period only.",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {addowner}={AA} {removeowner}={AA} {SignerArg}={AA}",

						Arguments =	[
										new ("<first>", "Id of an author to be renewed"),
										new (addowner, "Account Id of a new owner to add"),
										new (removeowner, "Account Id of a existing owner to remove"),
										new (SignerArg, "Address of account that owns the author")
									],

						Examples =	[
										new (null, $"{Keyword} {a.Name} {EID.Example} {addowner}={AA.Example1} {SignerArg}={AA.Example}")
									]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								
								if(Has(addowner))
								{
									return new AuthorOwnerAddition {AuthorId = FirstEntityId, Owner = GetAutoId(addowner)};
								}
								if(Has(removeowner))
								{
									return new AuthorOwnerRemoval {AuthorId = FirstEntityId, Owner = GetAutoId(addowner)};
								}
								else
									throw new SyntaxException("Unknown parameters");
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

								return new AuthorRenewal {AuthorId = FirstEntityId, Years = byte.Parse(GetString(years))};
							};
		return a;
	}
	
	public CommandAction Avatar()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var file = "file";

		a.Name = "avatar";
		a.Help = new() {Description = "",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {file}={EID} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of a author to update"),
									 new (file, "A file"),
									 new (SignerArg, "Address of account that is author's owner")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {file}={EID.Example1} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return	new AuthorAvatarChange
										{
											Author = FirstAuthorId,
											File = GetAutoId(file)
										}; 
							};
		return a;
	}

	public CommandAction Property()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var t = "title";
		var d = "description";

		a.Name = "p";
		a.Help = new() {Description = "Changes various author descriptive properties",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {t}={TEXT} {d}={TEXT} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of a author to update"),
									 new (t, "A new title"),
									 new (d, "A new description"),
									 new (SignerArg, "Address of account that owns the site")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {t}={TEXT.Example} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new AuthorTextChange();

								o.Author		= FirstAuthorId;
								o.Title			= GetString(t, null); 
								o.Description	= GetString(d, null); 

								return o;
							};
		return a;
	}

	public CommandAction Link()
	{
		var a = new CommandAction(MethodBase.GetCurrentMethod());
		
		var add = "add";
		var remove = "remove";

		a.Name = "link";
		a.Help = new() {Description = "Changes author's links",
						Syntax = $"{Keyword} {a.NamesSyntax} {EID} {add}={TEXT} {remove}={TEXT} {SignerArg}={AA}",

						Arguments =	[new ("<first>", "Id of a author to update"),
									 new (add, "A links to add"),
									 new (remove, "A links to remove"),
									 new (SignerArg, "Address of account that owns the author")],

						Examples =	[new (null, $"{Keyword} {a.Name} {EID.Example} {add}={URL.Example} {remove}={URL.Example1} {SignerArg}={AA.Example}")]};

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								var o = new AuthorLinksChange();

								o.Author		= FirstAuthorId;
								o.Additions		= Args.Where(i => i.Name == add).Select(i => i.Get<string>()).ToArray(); 
								o.Removals		= Args.Where(i => i.Name == remove).Select(i => i.Get<string>()).ToArray(); 

								return o;
							};
		return a;
	}
}
