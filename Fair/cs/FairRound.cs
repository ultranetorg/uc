namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv								Mcv => base.Mcv as FairMcv;
	public Dictionary<EntityId, AuthorEntry>		AffectedAuthors = new();
	public Dictionary<ProductId, ProductEntry>		AffectedProducts = new();
	public Dictionary<int, int>						NextAuthorIds = new ();
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
		if(table == Mcv.Accounts)
			return AffectedAccounts.Values;

		if(table == Mcv.Authors)
			return AffectedAuthors.Values;

		if(table == Mcv.Products)
			return AffectedProducts.Values;

		throw new IntegrityException();
	}

	public AuthorEntry AffectAuthor(EntityId id)
	{
		int ci;

		if(id == null)
		{
			if(Mcv.Authors.Clusters.Count() == 0)
			{	
				ci = 0;
				NextAuthorIds[ci] = 0;
			}
			else
			{	
				if(Mcv.Authors.Clusters.Count() < TableBase.ClustersCountMax)
				{	
					var i = Mcv.Authors.Clusters.Count();
					ci = (ushort)i;
					NextAuthorIds[ci] = 0;
				}
				else	
				{	
					var c = Mcv.Authors.Clusters.MinBy(i => i.Buckets.Count).Buckets.MinBy(i => i.Entries.Count());
					ci = c.Id;
					NextAuthorIds[ci] = c.NextEntryId;
				}
			}
				
			var pid = NextAuthorIds[ci]++;

			var a = new AuthorEntry(Mcv) {Id = new EntityId(ci, pid)};

			return AffectedAuthors[a.Id] = a;
		}
		else
		{
			if(AffectedAuthors.TryGetValue(id, out AuthorEntry a))
				return a;
			
			var e = Mcv.Authors.Find(id, Id - 1);

			if(e == null)
				throw new IntegrityException();

			AffectedAuthors[id] = e.Clone();
			//AffectedAuthors[id].Affected  = true;

			return AffectedAuthors[id];
		}
	}

	public ProductEntry AffectProduct(ProductId id)
	{
		if(AffectedProducts.TryGetValue(id, out var a))
			return a;
		
		a = Mcv.Products.Find(id, Id - 1);

		if(a == null)
			throw new IntegrityException();

		return AffectedProducts[id] = a.Clone();
	}

	public ProductEntry AffectProduct(AuthorEntry domain)
	{
		var d = AffectAuthor(domain.Id);

  		var	r = new ProductEntry  {Id = new ProductId(d.Id.H, d.Id.E, d.NextProductId++)};
    
  		return AffectedProducts[r.Id] = r;
	}

	public void DeleteProduct(ProductEntry resource)
	{
		AffectProduct(resource.Id).Deleted = true;
	}

	public override void InitializeExecution()
	{
	}

	public override void RestartExecution()
	{
		AffectedAuthors.Clear();
		AffectedProducts.Clear();
		NextAuthorIds.Clear();
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

