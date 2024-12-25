using Explorer.WebApi.Models.Responses.Operations;

namespace Explorer.WebApi.Models.Responses;

public class TransactionResponse : BaseRateResponse
{
	public string Id { get; set; } = null!;

	public string Signer { get; set; } = null!;

	public int Nid { get; set; }

	public BigInteger Fee { get; set; }

	public byte[]? Tag { get; set; }

	public int RoundId { get; set; }

	public int OperationsCount { get; set; }

	public ChildItemsResult<BaseOperationResponse> Operations { get; set; } = null!;
}
