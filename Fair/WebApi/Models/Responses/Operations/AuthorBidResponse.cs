namespace Explorer.WebApi.Models.Responses.Operations;

public class AuthorBidResponse : BaseOperationResponse
{
	public string Author { get; set; } = null!;

	public BigInteger Bid { get; set; }

	public string Tld { get; set; } = null!;
}
