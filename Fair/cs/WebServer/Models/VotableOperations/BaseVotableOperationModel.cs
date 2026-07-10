using System.Text.Json.Serialization;

namespace Uccs.Fair;

public abstract class BaseVotableOperationModel(StoreOperation operation)
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? SiteId { get; set; } = operation.Store?.Id.ToString();
}
