using Explorer.MongoDB.Documents.Resource;

namespace Explorer.WebApi.Models.Responses.Operations;

public class AnalysisResultUpdationResponse : BaseOperationResponse
{
	public ResourceAddress Resource { get; set; } = null!;

	public ResourceAddress Analysis { get; set; } = null!;

	public string Result { get; set; } = null!;
}
