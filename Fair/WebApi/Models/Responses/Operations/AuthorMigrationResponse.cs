namespace Explorer.WebApi.Models.Responses.Operations;

public class AuthorMigrationResponse : BaseOperationResponse
{
	public string Author { get; set; } = null!;

	public string Tld { get; set; } = null!;

	public bool RankCheck { get; set; }
}
