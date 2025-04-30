namespace Uccs.Fair;

public class FairRound : Round
{
	public new FairMcv										Mcv => base.Mcv as FairMcv;

	public Dictionary<AutoId, Author>						AffectedAuthors = new();
	public Dictionary<AutoId, Product>						AffectedProducts = new();
	public Dictionary<AutoId, Site>							AffectedSites = new();
	public Dictionary<AutoId, Category>						AffectedCategories = new();
	public Dictionary<AutoId, Publication>					AffectedPublications = new();
	public Dictionary<AutoId, Review>						AffectedReviews = new();
	public Dictionary<AutoId, Dispute>						AffectedDisputes = new();
	public Dictionary<RawId, Word>							AffectedWords = new();
	public PublicationTitleState							PublicationTitles;

	public FairRound(FairMcv mcv) : base(mcv)
	{
		PublicationTitles = new (mcv.PublicationTitles);
	}

	public override Execution CreateExecution(Transaction transaction)
	{
		return new FairExecution(Mcv, this, transaction);
	}

	public override long AccountAllocationFee(Account account)
	{
		return FairOperation.ToBD(Net.EntityLength, Uccs.Net.Mcv.Forever);
	}

	public override System.Collections.IDictionary AffectedByTable(TableBase table)
	{
		if(table == Mcv.Authors)			return AffectedAuthors;
		if(table == Mcv.Products)			return AffectedProducts;
		if(table == Mcv.Sites)				return AffectedSites;
		if(table == Mcv.Categories)			return AffectedCategories;
		if(table == Mcv.Publications)		return AffectedPublications;
		if(table == Mcv.Reviews)			return AffectedReviews;
		if(table == Mcv.Disputes)			return AffectedDisputes;
		if(table == Mcv.Words)				return AffectedWords;
		if(table == Mcv.PublicationTitles)	return PublicationTitles.Affected;

		return base.AffectedByTable(table);
	}

	public override S FindState<S>(TableBase table)
	{
		if(table == Mcv.PublicationTitles)	return PublicationTitles as S;

		return base.FindState<S>(table);
	}

	public override void Execute(IEnumerable<Transaction> transactions, bool trying = false)
	{
		AffectedAuthors.Clear();
		AffectedProducts.Clear();
		AffectedSites.Clear();
		AffectedCategories.Clear();
		AffectedPublications.Clear();
		AffectedReviews.Clear();
		AffectedDisputes.Clear();
		AffectedWords.Clear();

		//PublicationTitles = null;

		base.Execute(transactions, trying);
	}
	
	public override void Absorb(Execution execution)
	{
		base.Absorb(execution);

		var e = execution as FairExecution;

		foreach(var i in e.AffectedAuthors)				AffectedAuthors[i.Key] = i.Value;
		foreach(var i in e.AffectedProducts)			AffectedProducts[i.Key] = i.Value;
		foreach(var i in e.AffectedSites)				AffectedSites[i.Key] = i.Value;
		foreach(var i in e.AffectedCategories)			AffectedCategories[i.Key] = i.Value;
		foreach(var i in e.AffectedPublications)		AffectedPublications[i.Key] = i.Value;
		foreach(var i in e.AffectedReviews)				AffectedReviews[i.Key] = i.Value;
		foreach(var i in e.AffectedDisputes)			AffectedDisputes[i.Key] = i.Value;
		foreach(var i in e.AffectedWords)				AffectedWords[i.Key] = i.Value;

		PublicationTitles.Absorb(e.PublicationTitles);
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

