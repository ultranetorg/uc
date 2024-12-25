namespace Explorer.WebApi.Models.Responses.Operations;

public class CandidacyDeclarationResponse : BaseOperationResponse
{
	public BigInteger Bail { get; set; }

	public string[] BaseRdcIPs { get; set; } = null!;
	public string[] SeedHubRdcIPs { get; set; } = null!;
}
