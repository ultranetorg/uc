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
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "c";
		a.Description = "Creates a new author for a specified period";
		a.Arguments =  [new ("years", YEARS, "Number of years in [1..10] range"),
						new ("title", NAME, "A title of a author being created"),
						SignerArgument("Address of account that owns or is going to register the author")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
							};
		return a;
	}
	
	public CommandAction Nickname()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var nickname = "nickname";

		a.Name = "n";
		a.Description = "Sets an nickname for a specified author";
		a.Arguments =	[new (null, EID, "Id of a author to update", Flag.First),
						new (nickname, NAME, "A new nickname"),
						SignerArgument("Address of account that is author's owner")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorNicknameChange{Author = FirstAuthorId,
																Nickname = GetString(nickname)}; 
							};
		return a;
	}

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get author entity information from MCV database";
		a.Arguments =	[new (null, EID, "Id of an author to get information about", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcQueryTimeout);
				
								var rp = Ppc(new AuthorPpc(FirstAuthorId));

								Flow.Log.Dump(rp.Author);
					
								return rp.Author;
							};
		return a;
	}

	public CommandAction Update()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var addowner = "addowner";
		var removeowner = "removeowner";

		a.Name = "u";
		a.Description = "Extend author rent for a specified period. Allowed during the last year of current period only.";
		a.Arguments =	[
							new (null, EID, "Id of an author to be renewed", Flag.First),
							new (addowner, AA, "Account Id of a new owner to add"),
							new (removeowner, AA, "Account Id of a existing owner to remove"),
							SignerArgument("Address of account that owns the author")
						];

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
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var years = "years";

		a.Name = "r";
		a.Description = "Prolongs current expiration date of an author for a specified number of years";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (years, YEARS, "A number of years to renew author for. Allowed during the last year of current period only."),
						SignerArgument("Address of account that owns the author")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.RdcTransactingTimeout);

								return new AuthorRenewal {AuthorId = FirstEntityId, Years = byte.Parse(GetString(years))};
							};
		return a;
	}
	
	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var file = "file";

		a.Name = "avatar";
		a.Description = "Sets an avatar for a specified author";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (file, EID, "A file"),
						SignerArgument("Address of account that is author's owner")];

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
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var t = "title";
		var d = "description";

		a.Name = "p";
		a.Description = "Changes various author descriptive properties";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (t, TEXT, "A new title"),
						new (d, TEXT,  "A new description"),
						SignerArgument("Address of account that owns the site")];

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
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		var add = "add";
		var remove = "remove";

		a.Name = "link";
		a.Description = "Changes author's links";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (add, TEXT, "A links to add", Flag.Optional|Flag.Multi),
						new (remove, TEXT, "A links to remove", Flag.Optional|Flag.Multi),
						SignerArgument("Address of account that owns the author")];

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
