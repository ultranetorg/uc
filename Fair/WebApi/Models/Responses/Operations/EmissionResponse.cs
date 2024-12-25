namespace Explorer.WebApi.Models.Responses.Operations;

public class EmissionResponse : BaseOperationResponse
{
	public BigInteger Wei { get; set; }

	public int Eid { get; set; }
}
