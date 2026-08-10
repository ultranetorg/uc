namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv											Mcv => base.Mcv as FairMcv;

	public TableState<AutoId, Author>							Authors;
	public TableState<AutoId, Product>							Products;
	public TableState<AutoId, Store>							Stores;
	public TableState<AutoId, Category>							Categories;
	public TableState<AutoId, Publication>						Publications;
	public TableState<AutoId, Review>							Reviews;
	public TableState<AutoId, Proposal>							Proposals;
	public TableState<AutoId, ProposalComment>					ProposalComments;
	public TableState<AutoId, File>								Files;
	public TableState<StringId, TextToField<EntityTextField>>	Names;
	public NgramTableState<StoreNgramId>						StoreTitles;
	public NgramTableState<ProductNgramId>						ProductTitles;
	public NgramTableState<PublicationNgramId>					PublicationTitles;

	public FairRound(FairMcv mcv) : base(mcv)
	{
		Authors				= new (mcv.Authors);
		Products			= new (mcv.Products);
		Stores				= new (mcv.Stores);
		Categories			= new (mcv.Categories);
		Publications		= new (mcv.Publications);
		Reviews				= new (mcv.Reviews);
		Proposals			= new (mcv.Proposals);
		ProposalComments	= new (mcv.ProposalComments);
		Files				= new (mcv.Files);
		Names				= new (mcv.Names);
		StoreTitles			= new (mcv.StoreTitles);
		ProductTitles		= new (mcv.ProductTitles);
		PublicationTitles	= new (mcv.PublicationTitles);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new FairExecution(Mcv, this, transaction);
	}

	public override long UserAllocationFee()
	{
		return FairExecution.ToBD(Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)			return Authors.Affected;
		if(table == Mcv.Products)			return Products.Affected;
		if(table == Mcv.Stores)				return Stores.Affected;
		if(table == Mcv.Categories)			return Categories.Affected;
		if(table == Mcv.Publications)		return Publications.Affected;
		if(table == Mcv.Reviews)			return Reviews.Affected;
		if(table == Mcv.Proposals)			return Proposals.Affected;
		if(table == Mcv.ProposalComments)	return ProposalComments.Affected;
		if(table == Mcv.Files)				return Files.Affected;
		if(table == Mcv.Names)				return Names.Affected;
		if(table == Mcv.StoreTitles)		return StoreTitles.Affected;
		if(table == Mcv.ProductTitles)		return ProductTitles.Affected;
		if(table == Mcv.PublicationTitles)	return PublicationTitles.Affected;

		return base.AffectedByTable(table);
	}

	public override void ClearAffected()
	{
		base.ClearAffected();
		
		Authors.Affected.Clear();
		Products.Affected.Clear();
		Stores.Affected.Clear();
		Categories.Affected.Clear();
		Publications.Affected.Clear();
		Reviews.Affected.Clear();
		Proposals.Affected.Clear();
		Files.Affected.Clear();
		Names.Affected.Clear();
		StoreTitles.Affected.Clear();
		ProductTitles.Affected.Clear();
		PublicationTitles.Affected.Clear();
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.Authors)			return Authors as S;
		if(table == Mcv.Products)			return Products as S;
		if(table == Mcv.Stores)				return Stores as S;
		if(table == Mcv.Categories)			return Categories as S;
		if(table == Mcv.Publications)		return Publications as S;
		if(table == Mcv.Reviews)			return Reviews as S;
		if(table == Mcv.Proposals)			return Proposals as S;
		if(table == Mcv.ProposalComments)	return ProposalComments as S;
		if(table == Mcv.Files)				return Files as S;
		if(table == Mcv.Names)				return Names as S;
		if(table == Mcv.StoreTitles)		return StoreTitles as S;
		if(table == Mcv.ProductTitles)		return ProductTitles as S;
		if(table == Mcv.PublicationTitles)	return PublicationTitles as S;

		return base.FindState<S>(table);
	}
	
	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as FairExecution;

		Authors.Absorb(e.Authors);
		Products.Absorb(e.Products);
		Stores.Absorb(e.Stores);
		Categories.Absorb(e.Categories);
		Publications.Absorb(e.Publications);
		Reviews.Absorb(e.Reviews);
		Proposals.Absorb(e.Proposals);
		ProposalComments.Absorb(e.ProposalComments);
		Files.Absorb(e.Files);
		Names.Absorb(e.Words);
		StoreTitles.Absorb(e.StoreTitles);
		ProductTitles.Absorb(e.ProductTitles);
		PublicationTitles.Absorb(e.PublicationTitles);
	}

	public override void WriteGraphState(Writer writer)
	{
		base.WriteGraphState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.Write(writer));  
	}

	public override void ReadGraphState(Reader reader)
	{
		base.ReadGraphState(reader);

		Candidates	= reader.ReadList<Member>(() => { var g = new Member(); g.ReadCandidate(reader); return g;});
		Members		= reader.ReadList<Member>();
	}
}
