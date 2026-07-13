using System.Reflection;

namespace Uccs.Fair;

public class StoreCommand : FairCommand
{
	Argument Eligible => ByArgument("Name of the user eligible to propose changes to the store");

	public StoreCommand(FairCli program, List<Xon> args, Flow flow) : base(program, args, flow)
	{

	}

	public CommandAction Create()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		const string years = nameof(years);
		const string title = nameof(title);

		a.Name = "c";
		a.Description = "Creates a new store";
		a.Arguments =  [new (years, YEARS, "Number of years for which ownership is guaranteed"),
						new (title, NAME, "Title of the store being created"),
						ByArgument("Name of the user registering the store")];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new StoreCreation {Title = GetString(title), Years = byte.Parse(GetString(years))};
							};
		return a;
	}

	public CommandAction Renew()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());
		
		const string years = nameof(years);

		a.Name = "r";
		a.Description = "Prolongs expiration date of a store for the specified number of years";
		a.Arguments =  [ new (null, EID, "Id of a store to update", Flag.First),
						 new (years, YEARS, "A number of years (365 days per year) since today to renew the store for"),
						 Eligible];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.TransactingTimeout);

								return new StoreRenewal {StoreId = FirstAutoId, Years = byte.Parse(GetString(years))};
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
//		a.Arguments =  [new (null,		EID, "Id of the store to update", Flag.First),
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
//		a.Arguments =  [new (null,	EID, "Id of the store to update", Flag.First),
//						new (AsArg, ROLE, $"A role of actor, {Uccs.Fair.Role.Moderator} by default"),
//						new (null,	EID, "Id of the actor", Flag.Second),
//						new (t,		TEXT, "New title", Flag.Optional),
//						new (s,		TEXT, "New slogan", Flag.Optional),
//						new (d,		TEXT, "New description", Flag.Optional),
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
//		a.Arguments = [new (null, EID, "Id of the store to get categories from", Flag.First)];
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

	public CommandAction Entity()
	{
		var a = new CommandAction(this, MethodBase.GetCurrentMethod());

		a.Name = "e";
		a.Description = "Get information about the specified store";
		a.Arguments =	[new (null, EID, "Id of the store to get information about", Flag.First)];

		a.Execute = () =>	{
								Flow.CancelAfter(Cli.Settings.PpcTimeout);
				
								var rp = Ppc(new StorePpc(FirstAutoId));

								Flow.Log.Dump(rp.Store);
					
								return rp.Store;
							};
		return a;
	}
}
