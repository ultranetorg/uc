using Explorer.MongoDB.Documents.Resource;

namespace Explorer.WebApi.Models.Responses.Operations;

public class ResourceLinkCreationResponse : BaseOperationResponse
{
	public ResourceAddress Source { get; set; } = null!;
	public ResourceAddress Destination { get; set; } = null!;

	public Uccs.Net.ResourceLinkFlag Changes { get; set; }
}
