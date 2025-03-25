namespace Uccs.Fair;

public class TextTable : Table<Text>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public TextTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override Text Create()
	{
		return new Text(Mcv);
	}
 }
