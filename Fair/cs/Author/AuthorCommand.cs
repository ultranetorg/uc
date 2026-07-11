using System.Reflection;

namespace Uccs.Fair;

public class AuthorCommand : FairCommand
{
	AutoId FirstAuthorId => AutoId.Parse(First);

	Argument Eligible => ByArgument("A name of user eligible to change Author entity");

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
						ByArgument("Address of account that owns or is going to register the author")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
							};
		return a;
	}
	
	public CommandAction Name()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string name = "name";

		a.Name = "n";
		a.Description = "Sets an nickname for a specified author";
		a.Arguments =	[new (null, EID, "Id of a author to update", Flag.First),
						new (name, NAME, "A new nickname"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new AuthorNameChange
										{
											Author = FirstAuthorId,
											Name = GetString(name)
										}; 
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
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new AuthorPpc(FirstAuthorId));

								Flow.Log.Dump(rp.Author);
					
								return rp.Author;
							};
		return a;
	}

	public CommandAction Security()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string ao = "addowner";
		const string ro = "removeowner";

		a.Name = "s";
		a.Description = "Manages ownership of an author. Adds or removes an owner account.";
		a.Arguments =	[
							new (null, EID, "Author ID whose owner list will be modified", Flag.First),
							new (ao, AA, "Account ID to add as an owner"),
							new (ro, AA, "Account ID to remove from owners"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);
								
								FairOperation o = null;
								
								if(Has(ao))
								{
									o = new AuthorOwnerAddition {AuthorId = FirstAutoId, Owner = GetAutoId(ao)};
								}
								if(Has(ro))
								{
									o = new AuthorOwnerRemoval {AuthorId = FirstAutoId, Owner = GetAutoId(ao)};
								}

								return o ?? throw new SyntaxException("Unknown parameters");
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string years = "years";

		a.Name = "r";
		a.Description = "Prolongs current expiration date of an author for a specified number of years";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (years, YEARS, "A number of years to renew author for. "),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorRenewal {AuthorId = FirstAutoId, Years = byte.Parse(GetString(years))};
							};
		return a;
	}
		
	public CommandAction PulisherLimits()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string store = "store";
		const string energy = "energy";
		const string spacetime = "spacetime";

		a.Name = "pl";
		a.Description = "Sets a utility limits for the specified author of the specfied store";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (store, EID, "Id of a store where author is the member"),
						new (energy, INT, "A new limit for the energy"),
						new (spacetime, INT, "A new limit for the spacetime"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new PublisherLimitsUpdation {Author = FirstAutoId, Store = GetAutoId(store), EnergyLimit = GetLong(energy), SpacetimeLimit = GetLong(spacetime)};
							};
		return a;
	}
		
	public CommandAction ModerationReward()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string energy = "energy";

		a.Name = "mr";
		a.Description = "Sets the moderation reward for the specified author";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (energy, INT, "Amount of energy for reward"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorModerationReward {AuthorId = FirstAutoId, Energy = GetLong(energy)};
							};
		return a;
	}
		
	public CommandAction Verification()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string domain = "domain";

		a.Name = "v";
		a.Description = "Requests the net to check ownership of web domain for the specified author";
		a.Arguments =  [new (null, EID, "Id of an author to verify", Flag.First),
						new (domain, NAME, "Web domain name"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorVerification {Author = FirstAutoId, Webdomain = GetString(domain)};
							};
		return a;
	}

	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string file = "file";

		a.Name = "avatar";
		a.Description = "Sets an avatar for a specified author. Only files with correct media type(MIME) are accepted.";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (file, EID, "Id of a file entity"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new AuthorAvatarChange
										{
											Author = FirstAuthorId,
											File = GetAutoId(file)
										}; 
							};
		return a;
	}

	public CommandAction Info()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string t = "title";
		const string d = "description";
		const string r = "ref";

		a.Name = "i";
		a.Description = "Changes various author descriptive properties";
		a.Arguments =  [new (null, EID, "Id of a author to update", Flag.First),
						new (t, TEXT, "A new title"),
						new (d, TEXT,  "A new description"),
						new (r, new ArgumentType("{text=TEXT uri=URI}", null, ["{text=Website uri=http://www.company.com}", "{text=Github uri=http://github.com/company}"]), "Zero or more Uri references with a description"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var o = new AuthorInfoUpdation();

								o.Author = FirstAuthorId;
								o.Changes = [];

								foreach(var i in Args)
								{
									switch(i.Name)
									{
										case t : o.Changes = [..o.Changes, new (AuthorField.Title,		 i.Get<string>())]; break;
										case d : o.Changes = [..o.Changes, new (AuthorField.Description, i.Get<string>())]; break;
										case r : o.Changes = [..o.Changes, new (AuthorField.Reference,	 i.Get<Xon>().Get<string>("text"), i.Get<Xon>().Get<string>("uri"))]; break;
									}
								}

								return o;
							};
		return a;
	}
}
