namespace Uccs.Fair;

public class PublicationTable : Table<PublicationEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public PublicationTable(FairMcv rds) : base(rds)
	{
	}
	
	public override PublicationEntry Create()
	{
		return new PublicationEntry(Mcv);
	}
 }
