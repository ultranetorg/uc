namespace Uccs.Fair;

public class TextSearchResult
{
	public string		Text { get; set; }
	public EntityId		Entity { get; set; }

	public override string ToString()
	{
		return $"{Text}, {Entity}";
	}
}

public class WordTable : Table<Word>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;
	public override bool			IsIndex => true;

	public WordTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Word Create()
	{
		return new Word(Mcv);
	}
 }
