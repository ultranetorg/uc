using Explorer.WebApi.Models.Responses.Operations;

namespace Explorer.WebApi.Models.Responses;

public class AccountResponse : BaseRateResponse
{
	public string Address { get; init; } = null!;

	public BigInteger Balance { get; set; }

	// public BigInteger Bail { get; set; }

	public int LastTransactionNid { get; set; } = -1;
	public int LastEmissionId { get; set; } = -1;
	// public int CandidacyDeclarationRid { get; set; } = -1;

	public BigInteger AverageUptime { get; set; }

	// Manually.

	public List<AccountAuthorModel> Authors { get; set; } = null!;

	public ChildItemsResult<BaseOperationResponse> Operations { get; set; } = null!;
}
