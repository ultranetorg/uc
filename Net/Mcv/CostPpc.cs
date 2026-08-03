
namespace Uccs.Net;

public class CostPpc : McvPpc<CostPpr>
{
	public override Result Execute()
	{
		RequireGraph();
		
		return new CostPpr{//RentPerBytePerDay = Mcv.LastConfirmedRound.RentPerBytePerDay,
								ConsensusExecutionFee = Mcv.LastConfirmedRound.ConsensusOperationCost};
	}

	public override void Read(Reader reader)
	{
	}

	public override void Write(Writer writer)
	{
	}
}

public class CostPpr : Result
{
	public long ConsensusExecutionFee { get; set; }

	public override void Read(Reader reader)
	{
		ConsensusExecutionFee = reader.Read7BitEncodedInt64();
	}

	public override void Write(Writer writer)
	{
		writer.Write7BitEncodedInt64(ConsensusExecutionFee);
	}

}

