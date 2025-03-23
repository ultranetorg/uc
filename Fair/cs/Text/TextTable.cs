namespace Uccs.Fair;

public class NicknameTable : Table<TextEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public NicknameTable(FairMcv mcv) : base(mcv)
	{
	}
	
	public override TextEntry Create()
	{
		return new TextEntry(Mcv);
	}
 }
