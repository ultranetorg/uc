namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv								Mcv => base.Mcv as FairMcv;
	public Dictionary<EntityId, AuthorEntry>		AffectedAuthors = new();
	public Dictionary<EntityId, ProductEntry>		AffectedProducts = new();
	public Dictionary<EntityId, CatalogueEntry>		AffectedCatalogues = new();
	public Dictionary<EntityId, TopicEntry>		AffectedTopics = new();
	public Dictionary<int, int>						NextAuthorEids = new ();
	public Dictionary<int, int>						NextProductEids = new ();
	public Dictionary<int, int>						NextCatalogueEids = new ();
	public Dictionary<int, int>						NextTopicEids = new ();
	//public Dictionary<ushort, int>					NextAssortmentIds = new ();

	public FairRound(FairMcv rds) : base(rds)
	{
	}

	public override long AccountAllocationFee(Account account)
	{
		return FairOperation.SpacetimeFee(Uccs.Net.Mcv.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)	return AffectedAuthors;
		if(table == Mcv.Products)	return AffectedProducts;
		if(table == Mcv.Catalogues)	return AffectedCatalogues;
		if(table == Mcv.Topics)		return AffectedTopics;

		return base.AffectedByTable(table);
	}

	public override Dictionary<int, int> NextEidsByTable(TableBase table)
	{
		if(table == Mcv.Authors)	return NextAuthorEids;
		if(table == Mcv.Products)	return NextProductEids;
		if(table == Mcv.Catalogues)	return NextCatalogueEids;
		if(table == Mcv.Topics)		return NextTopicEids;
		//if(table == Mcv.Resources)	return AffectedResources.Values;

		return base.NextEidsByTable(table);
	}

	public new FairAccountEntry AffectAccount(AccountAddress address)
	{
		return base.AffectAccount(address) as FairAccountEntry;
	}

	public new FairAccountEntry AffectAccount(EntityId id)
	{
		return base.AffectAccount(id) as FairAccountEntry;
	}

	public AuthorEntry AffectAuthor(AccountAddress signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer);
		
		int e = GetNextEid(Mcv.Authors, b);

		var a = Mcv.Authors.Create();
		a.Id = new EntityId(b, e);
			
		return AffectedAuthors[a.Id] = a;
	}

	public AuthorEntry AffectAuthor(EntityId id)
	{
		if(AffectedAuthors.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Authors.Find(id, Id - 1);

		return AffectedAuthors[id] = e.Clone();
	}

	public ProductEntry AffectProduct(AuthorEntry author)
	{
		int e = GetNextEid(Mcv.Products, author.Id.B);

  		var	p = new ProductEntry {Id = new EntityId(author.Id.B, e)};
    
  		return AffectedProducts[p.Id] = p;
	}

	public ProductEntry AffectProduct(EntityId id)
	{
		if(AffectedProducts.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Products.Find(id, Id - 1);

		return AffectedProducts[id] = a.Clone();
	}

	public CatalogueEntry AffectCatalogue(AccountEntry signer)
	{
		var b = Mcv.Accounts.KeyToBid(signer.Address);
		
		int e = GetNextEid(Mcv.Catalogues, b);

		var a = Mcv.Catalogues.Create();
		a.Id = new EntityId(b, e);
			
		return AffectedCatalogues[a.Id] = a;
	}

	public CatalogueEntry AffectCatalogue(EntityId id)
	{
		if(AffectedCatalogues.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Catalogues.Find(id, Id - 1);

		return AffectedCatalogues[id] = e.Clone();
	}

	public TopicEntry AffectTopic(CatalogueEntry catalogue)
	{
		int e = GetNextEid(Mcv.Topics, catalogue.Id.B);

		var a = Mcv.Topics.Create();
		a.Id = new EntityId(catalogue.Id.B, e);
			
		return AffectedTopics[a.Id] = a;
	}

	public TopicEntry AffectTopic(EntityId id)
	{
		if(AffectedTopics.TryGetValue(id, out var a))
			return a;
			
		var e = Mcv.Topics.Find(id, Id - 1);

		return AffectedTopics[id] = e.Clone();
	}

	public override void RestartExecution()
	{
// 		AffectedAuthors.Clear();
// 		AffectedProducts.Clear();
// 		AffectedCatalogues.Clear();
// 		AffectedCards.Clear();
// 
// 		NextAuthorEids.Clear();
// 		NextProductEids.Clear();
// 		NextCatalogueEids.Clear();
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

	public override void ConfirmForeign()
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

