namespace Explorer.WebApi.Models.Responses.Operations;

public class AuthorTransferResponse : BaseOperationResponse
{
	public string Author { get; set; } = null!;

	public string To { get; set; } = null!;
}
