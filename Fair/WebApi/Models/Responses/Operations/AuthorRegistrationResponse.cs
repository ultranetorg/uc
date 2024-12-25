namespace Explorer.WebApi.Models.Responses.Operations;

public class AuthorRegistrationResponse : BaseOperationResponse
{
	public string Author { get; set; } = null!;

	public byte Years { get; set; }
}
