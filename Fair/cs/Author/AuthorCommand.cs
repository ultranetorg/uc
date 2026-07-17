using System.Reflection;

namespace Uccs.Fair;

public class AuthorCommand : FairCommand
{
	//AutoId					FirstAuthorId => AutoId.Parse(First);
	public static Argument	Eligible => ByArgument("Name of the user eligible to change Author entity");

	new AutoId Id
	{
		get
		{
			if(Has(IdKeyword))
				return GetAutoId(IdKeyword);
			else if(Has(NameKeyword))
				return Ppc(new AuthorByNamePpc(GetString(NameKeyword))).Author.Id;
			else
				throw new SyntaxException("Neither author 'id' nor 'name' arguments provided");
		}
	}

	public AuthorCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Creates a new author for the specified period";
		a.Arguments =  [new ("years", YEARS, "Number of years in [1..10] range"),
						new ("title", NAME, "A title of a author being created"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorCreation {Title = GetString("title"), Years = byte.Parse(GetString("years"))};
							};
		return a;
	}
	
	public CommandAction Name_N()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string name = "name";

		a.Description = "Sets an nickname for the specified author";
		a.Arguments =  [
							NameOrId("author to set name for"),
							new (name, NAME, "New name"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new AuthorNameChange
										{
											Author = Id,
											Name = GetString(name)
										}; 
							};
		return a;
	}

	public CommandAction Security_S()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string ao = "addowner";
		const string ro = "removeowner";

		a.Description = "Manages ownership of an author. Adds or removes an owner account.";
		a.Arguments =	[
							NameOrId("author whose owner list will be modified"),
							new (ao, AA, "User Id to add as an owner"),
							new (ro, AA, "User Id to remove from owners"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);
								
								FairOperation o = null;
								
								if(Has(ao))
								{
									o = new AuthorOwnerAddition {AuthorId = Id, Owner = GetAutoId(ao)};
								}
								if(Has(ro))
								{
									o = new AuthorOwnerRemoval {AuthorId = Id, Owner = GetAutoId(ro)};
								}

								return o ?? throw new SyntaxException("Unknown parameters");
							};
		return a;
	}

	public CommandAction Renew_R()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string years = "years";

		a.Description = "Prolongs current expiration date of an author for the specified number of years";
		a.Arguments =  [NameOrId("author to update"),
						new (years, YEARS, "The number of years to renew author for. "),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorRenewal {AuthorId = Id, Years = byte.Parse(GetString(years))};
							};
		return a;
	}
		
	public CommandAction PulisherLimits_PL()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string store = "store";
		const string energy = "energy";
		const string spacetime = "spacetime";

		a.Description = "Sets a utility limits for the specified author of the specfied store";
		a.Arguments =  [NameOrId("author to update"),
						new (store, EID, "Id of a store where author is the member"),
						new (energy, INT, "New limit for the energy"),
						new (spacetime, INT, "New limit for the spacetime"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new PublisherLimitsUpdation {Author = Id, Store = GetAutoId(store), EnergyLimit = GetLong(energy), SpacetimeLimit = GetLong(spacetime)};
							};
		return a;
	}
		
	public CommandAction ModerationReward_MR()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string energy = "energy";

		a.Description = "Sets the moderation reward for the specified author";
		a.Arguments =  [NameOrId("author to update"),
						new (energy, INT, "Amount of energy for reward"),
						Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorModerationReward {AuthorId = Id, Energy = GetLong(energy)};
							};
		return a;
	}
		
	public CommandAction Verification_V()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string domain = "domain";

		a.Description = "Requests the net to check ownership of web domain for the specified author";
		a.Arguments =  [
							NameOrId("author to verify"),
							new (domain, NAME, "Web domain name"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new AuthorVerification {Author = Id, Webdomain = GetString(domain)};
							};
		return a;
	}

	public CommandAction Avatar()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string file = "file";

		a.Description = "Sets an avatar for the specified author. Only files with correct media type(MIME) are accepted.";
		a.Arguments =	[
							NameOrId("author to change avatar of"),
							new (file, EID, "Id of a file entity"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return	new AuthorAvatarChange
										{
											Author = Id,
											File = GetAutoId(file)
										}; 
							};
		return a;
	}

	public CommandAction Info_i()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string t = "title";
		const string d = "description";
		const string r = "ref";

		a.Description = "Changes various author descriptive properties";
		a.Arguments =	[
							NameOrId("author to update"),
							new (t, TEXT, "New title"),
							new (d, TEXT,  "New description"),
							new (r, HYPERLINK, "Zero or more Uri references with a description"),
							Eligible
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								var o = new AuthorInfoUpdation();

								o.Author = Id;
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

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get author entity information from MCV database";
		a.Arguments =	[NameOrId("author to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								Author e;

								if(Has(IdKeyword))
									e = Ppc(new AuthorPpc(Id)).Author;
								else if(Has(NameKeyword))
									e = Ppc(new AuthorByNamePpc(Name)).Author;
								else
									throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");

								Flow.Log.Dump(e);
								
								return e;
							};
		return a;
	}

}
