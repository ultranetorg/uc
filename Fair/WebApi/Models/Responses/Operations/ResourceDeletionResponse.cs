using Explorer.MongoDB.Documents.Resource;

namespace Explorer.WebApi.Models.Responses.Operations;

public class ResourceDeletionResponse : BaseOperationResponse
{
	public ResourceAddress Resource { get; set; } = null!;
}
