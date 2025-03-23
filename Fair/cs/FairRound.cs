
namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv							Mcv => base.Mcv as FairMcv;
	public Dictionary<EntityId, Author>			AffectedAuthors = new();
	public Dictionary<EntityId, Product>		AffectedProducts = new();
	public Dictionary<EntityId, Site>			AffectedSites = new();
	public Dictionary<EntityId, Category>		AffectedCategories = new();
	public Dictionary<EntityId, Publication>	AffectedPublications = new();
	public Dictionary<EntityId, Review>			AffectedReviews = new();
	public Dictionary<EntityId, Dispute>		AffectedDisputes = new();
	public Dictionary<StringId, Text>			AffectedNicknames = new();

	public FairRound(FairMcv rds) : base(rds)
	{
	}

	public override long AccountAllocationFee(Account account)
	{
		return FairOperation.ToBD(Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)		return AffectedAuthors;
		if(table == Mcv.Products)		return AffectedProducts;
		if(table == Mcv.Sites)			return AffectedSites;
		if(table == Mcv.Categories)		return AffectedCategories;
		if(table == Mcv.Publications)	return AffectedPublications;
		if(table == Mcv.Reviews)		return AffectedReviews;
		if(table == Mcv.Disputes)		return AffectedDisputes;
		if(table == Mcv.Nicknames)		return AffectedNicknames;

		return base.AffectedByTable(table);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new FairExecution(Mcv, this, transaction);
	}
	
	public override void Consume(Execution execution)
	{
		base.Consume(execution);

		var e = execution as FairExecution;

		foreach(var i in e.AffectedAuthors)			AffectedAuthors[i.Key] = i.Value;
		foreach(var i in e.AffectedProducts)		AffectedProducts[i.Key] = i.Value;
		foreach(var i in e.AffectedSites)			AffectedSites[i.Key] = i.Value;
		foreach(var i in e.AffectedCategories)		AffectedCategories[i.Key] = i.Value;
		foreach(var i in e.AffectedPublications)	AffectedPublications[i.Key] = i.Value;
		foreach(var i in e.AffectedReviews)			AffectedReviews[i.Key] = i.Value;
		foreach(var i in e.AffectedDisputes)		AffectedDisputes[i.Key] = i.Value;
		foreach(var i in e.AffectedNicknames)		AffectedNicknames[i.Key] = i.Value;
	}

// 	public bool IsAllowed(Publication page, TopicChange change, AccountEntry signer)
// 	{
// 		if(page == null)
// 		{
// 			return Mcv.Sites.Find(page.Site, Id)?.Owners.Contains(signer.Id) ?? false;
// 		}
// 
// 		if(page.Security != null)
// 		{
// 			if(page.Security.Permissions.TryGetValue(change, out var a))
// 			{
// 				return a.Any(i =>	{
// 										switch(i)
// 										{
// 											case Actor.Owner:
// 												return Mcv.Sites.Find(page.Site, Id)?.Owners.Contains(signer.Id) ?? false;
// 
// 											case Actor.Creator:
// 												return page.Creator == signer.Id;
// 
// 											case Actor.SiteUser :
// 											{
// 												throw new NotImplementedException();
// 												break;
// 											}
// 
// 											default:
// 												return false;
// 										}
// 									});
// 			}
// 			else
// 				return false;
// 		}
// 
// 		if(Parent != null)
// 		{
// 			var p = Mcv.Publications.Find(page.Parent, Id);
// 
// 			return IsAllowed(p, change, signer);
// 		}
// 
// 		return false;
// 	}
// 
// 	public bool NotPermitted(Publication page, TopicChange permission, AccountEntry signer)
// 	{
// 		return !IsAllowed(page, permission, signer);
// 	}

	public override void RestartExecution()
	{
// 		AffectedAuthors.Clear();
// 		AffectedProducts.Clear();
// 		AffectedSites.Clear();
// 		AffectedCards.Clear();
// 
// 		NextAuthorEids.Clear();
// 		NextProductEids.Clear();
// 		NextSiteEids.Clear();
// 		NextCardEids.Clear();
	}

	public override void FinishExecution()
	{
	}

	public override void Elect(Vote[] votes, int gq)
	{
	}

	public override void CopyConfirmed()
	{
	}

	public override void RegisterForeign(Operation o)
	{
	}

	public override void ConfirmForeign(Execution execution)
	{
	}

	public override void WriteBaseState(BinaryWriter writer)
	{
		base.WriteBaseState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
	}

	public override void ReadBaseState(BinaryReader reader)
	{
		base.ReadBaseState(reader);

		Candidates	= reader.Read<Generator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<Generator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
	}

}

