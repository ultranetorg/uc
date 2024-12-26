namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv								Mcv => base.Mcv as FairMcv;
	public Dictionary<EntityId, AuthorEntry>		AffectedAuthors = new();
	public Dictionary<EntityId, ProductEntry>		AffectedProducts = new();
	public Dictionary<int, int>						NextAuthorEids = new ();
	public Dictionary<int, int>						NextProductEids = new ();
	//public Dictionary<ushort, int>					NextAssortmentIds = new ();

	public FairRound(FairMcv rds) : base(rds)
	{
	}

	public override long AccountAllocationFee(Account account)
	{
		return FairOperation.SpacetimeFee(Uccs.Net.Mcv.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override IEnumerable<object> AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)	return AffectedAuthors.Values;
		if(table == Mcv.Products)	return AffectedProducts.Values;

		return base.AffectedByTable(table);
	}

	public override Dictionary<int, int> NextEidsByTable(TableBase table)
	{
		if(table == Mcv.Authors)	return NextAuthorEids;
		if(table == Mcv.Products)	return NextProductEids;
		//if(table == Mcv.Resources)	return AffectedResources.Values;

		return base.NextEidsByTable(table);
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

	public void DeleteProduct(ProductEntry resource)
	{
		AffectProduct(resource.Id).Deleted = true;
	}

	public override void RestartExecution()
	{
		AffectedAuthors.Clear();
		AffectedProducts.Clear();
		NextAuthorEids.Clear();
		NextProductEids.Clear();
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

