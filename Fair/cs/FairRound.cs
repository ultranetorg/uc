namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv											Mcv => base.Mcv as FairMcv;

	public TableState<AutoId, Author>							Authors;
	public TableState<AutoId, Product>							Products;
	public TableState<AutoId, Site>								Sites;
	public TableState<AutoId, Category>							Categories;
	public TableState<AutoId, Publication>						Publications;
	public TableState<AutoId, Review>							Reviews;
	public TableState<AutoId, Proposal>							Proposals;
	public TableState<AutoId, ProposalComment>					ProposalComments;
	public TableState<AutoId, File>								Files;
	public TableState<RawId, Word>								Words;
	public HnswTableState<string, StringToDictionaryHnswEntity>	PublicationTitles;
	public HnswTableState<string, StringToOneHnswEntity>		SiteTitles;

	public FairRound(FairMcv mcv) : base(mcv)
	{
		Authors				= new (mcv.Authors);
		Products			= new (mcv.Products);
		Sites				= new (mcv.Sites);
		Categories			= new (mcv.Categories);
		Publications		= new (mcv.Publications);
		Reviews				= new (mcv.Reviews);
		Proposals			= new (mcv.Proposals);
		ProposalComments	= new (mcv.ProposalComments);
		Files				= new (mcv.Files);
		Words				= new (mcv.Words);
		PublicationTitles	= new (mcv.PublicationTitles);
		SiteTitles			= new (mcv.SiteTitles);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new FairExecution(Mcv, this, transaction);
	}

	public override long AccountAllocationFee()
	{
		return FairExecution.ToBD(Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)			return Authors.Affected;
		if(table == Mcv.Products)			return Products.Affected;
		if(table == Mcv.Sites)				return Sites.Affected;
		if(table == Mcv.Categories)			return Categories.Affected;
		if(table == Mcv.Publications)		return Publications.Affected;
		if(table == Mcv.Reviews)			return Reviews.Affected;
		if(table == Mcv.Proposals)			return Proposals.Affected;
		if(table == Mcv.ProposalComments)	return ProposalComments.Affected;
		if(table == Mcv.Files)				return Files.Affected;
		if(table == Mcv.Words)				return Words.Affected;
		if(table == Mcv.PublicationTitles)	return PublicationTitles.Affected;
		if(table == Mcv.SiteTitles)			return SiteTitles.Affected;

		return base.AffectedByTable(table);
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.Authors)			return Authors as S;
		if(table == Mcv.Products)			return Products as S;
		if(table == Mcv.Sites)				return Sites as S;
		if(table == Mcv.Categories)			return Categories as S;
		if(table == Mcv.Publications)		return Publications as S;
		if(table == Mcv.Reviews)			return Reviews as S;
		if(table == Mcv.Proposals)			return Proposals as S;
		if(table == Mcv.ProposalComments)	return ProposalComments as S;
		if(table == Mcv.Files)				return Files as S;
		if(table == Mcv.Words)				return Words as S;
		if(table == Mcv.PublicationTitles)	return PublicationTitles as S;
		if(table == Mcv.SiteTitles)			return SiteTitles as S;

		return base.FindState<S>(table);
	}
	
	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as FairExecution;

		Authors.Absorb(e.Authors);
		Products.Absorb(e.Products);
		Sites.Absorb(e.Sites);
		Categories.Absorb(e.Categories);
		Publications.Absorb(e.Publications);
		Reviews.Absorb(e.Reviews);
		Proposals.Absorb(e.Proposals);
		ProposalComments.Absorb(e.ProposalComments);
		Files.Absorb(e.Files);
		Words.Absorb(e.Words);
		PublicationTitles.Absorb(e.PublicationTitles);
		SiteTitles.Absorb(e.SiteTitles);
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

	public override void WriteGraphState(BinaryWriter writer)
	{
		base.WriteGraphState(writer);

		writer.Write(Candidates, i => i.WriteCandidate(writer));  
		writer.Write(Members, i => i.WriteMember(writer));  
	}

	public override void ReadGraphState(BinaryReader reader)
	{
		base.ReadGraphState(reader);

		Candidates	= reader.Read<Generator>(m => m.ReadCandidate(reader)).Cast<Generator>().ToList();
		Members		= reader.Read<Generator>(m => m.ReadMember(reader)).Cast<Generator>().ToList();
	}

}

