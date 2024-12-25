namespace Explorer.WebApi.Models.Responses.Operations;

public class UntTransferResponse : BaseOperationResponse
{
	public string To { get; set; } = null!;

	public BigInteger Amount { get; set; }
}
