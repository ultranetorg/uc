using Explorer.MongoDB.Documents.Resource;

namespace Explorer.WebApi.Models.Responses.Operations;

public class ResourceCreationResponse : BaseOperationResponse
{
	public ResourceAddress Resource { get; set; } = null!;

	public string Flags { get; set; } = null!;

	public ResourceData? Data { get; set; }
}
