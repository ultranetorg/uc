namespace Uccs.Fair;

public class WordTable : Table<RawId, Word>
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
