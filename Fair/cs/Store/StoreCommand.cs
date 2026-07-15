using System.Reflection;

namespace Uccs.Fair;

public class StoreCommand : FairCommand
{
	Argument	Eligible => ByArgument("Name of the user eligible to propose changes to the store");

	new AutoId Id
	{
		get
		{
			if(Has(IdKeyword))
				return GetAutoId(IdKeyword);
			else if(Has(NameKeyword))
				return Ppc(new StoreByNamePpc(GetString(NameKeyword))).Store.Id;
			else
				throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");
		}
	}

	public StoreCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create_C()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string years = nameof(years);
		const string title = nameof(title);

		a.Description = "Creates a new store";
		a.Arguments =  [
							new (years, YEARS, "Number of years for which ownership is guaranteed"),
							new (title, NAME, "Title of the store being created"),
							ByArgument("Name of the user registering the store")
						];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new StoreCreation {Title = GetString(title), Years = byte.Parse(GetString(years))};
							};
		return a;
	}

	public CommandAction Renew_R()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string years = nameof(years);

		a.Description = "Prolongs expiration date of a store for the specified number of years";
		a.Arguments =	[	
							NameOrId(NAME,  "store to update"),
							new (years, YEARS, "Number of years (365 days per year) since today to renew the store for"),
							Eligible
						 ];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new StoreRenewal {StoreId = Id, Years = byte.Parse(GetString(years))};
							};
		return a;
	}

//	public CommandAction Name()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//		
//		const string name = nameof(name);
//		const string creator = nameof(creator);
//
//		a.Name = "n";
//		a.Description = "Creates a proposal to change a name for the specified store";
//		a.Arguments =  [new (null,		EID, "Id of the store to update", ArgumentFlag.First),
//						new (AsArg,		ROLE, "On behalf of"),
//						new (creator,	EID, "Id of the creator"),
//						new (name,		EID, "New name"),
//						Eligible];
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.TransactingTimeout);
//
//								return new ProposalCreation(FirstAutoId, SecondAutoId, GetEnum<Role>(AsArg), new StoreNameChange {Name = GetString(name)}); 
//							};
//		return a;
//	}

//	public CommandAction Info()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//		
//		const string t = "title";
//		const string s = "slogan";
//		const string d = "description";
//
//		a.Name = "i";
//		a.Description = "Changes information about a store";
//		a.Arguments =  [new (null,	EID, "Id of the store to update", ArgumentFlag.First),
//						new (AsArg, ROLE, $"A role of actor, {Uccs.Fair.Role.Moderator} by default"),
//						new (null,	EID, "Id of the actor", Flag.Second),
//						new (t,		TEXT, "New title", ArgumentFlag.Optional),
//						new (s,		TEXT, "New slogan", ArgumentFlag.Optional),
//						new (d,		TEXT, "New description", ArgumentFlag.Optional),
//						Eligible];
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.TransactingTimeout);
//
//								var o = new StoreInfoUpdation();
//
//								o.Title			= GetString(t, null); 
//								o.Slogan		= GetString(s, null); 
//								o.Description	= GetString(d, null); 
//
//								return new ProposalCreation(FirstAutoId, SecondAutoId, GetEnum<Role>(AsArg, Uccs.Fair.Role.Moderator), o);
//							};
//		return a;
//	}

//	public CommandAction ListCategories()
//	{
//		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
//
//		a.Name = "lc";
//		a.Description = "Get categories of the specified store";
//		a.Arguments = [new (null, EID, "Id of the store to get categories from", ArgumentFlag.First)];
//
//		a.Execute = () =>	{
//								Flow.CancelAfter(Cli.Settings.PpcTimeout);
//				
//								var rp = Ppc(new StoreCategoriesPpc(FirstAutoId));
//
//								Flow.Log.DumpFixed(rp.Categories.Select(i => Ppc(new CategoryPpc(i)).Category), ["Id", "Title", "Categories"], [i => i.Id, i => i.Title, i => i.Categories?.Length]);
//					
//								return rp.Categories;
//							};
//		return a;
//	}

	public CommandAction Entity_E()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Description = "Get information about the specified store";
		a.Arguments =	[NameOrId(NAME, "store to get information about")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								Store u;

								if(Has(IdKeyword))
									u = Ppc(new StorePpc(Id)).Store;
								else if(Has(NameKeyword))
									u = Ppc(new StoreByNamePpc(Name)).Store;
								else
									throw new SyntaxException("Neither domain 'id' nor 'name' arguments provided");

								Flow.Log.Dump(u);
					
								return u;
							};
		return a;
	}
}
