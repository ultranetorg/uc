using UO.DomainModels.Operations;

namespace UC.DomainModels;

public class TransactionModel
{
	public string Id { get; set; } = null!;

	public string Generator { get; set; } = null!;

	public string Signer { get; set; } = null!;

	public int Nid { get; set; }

	public long ECFee { get; set; }

	public byte[]? Tag { get; set; }

	public int RoundId { get; set; }

	public int OperationsCount { get; set; }

	public IEnumerable<BaseOperationModel> Operations { get; set; } = null!;
}
