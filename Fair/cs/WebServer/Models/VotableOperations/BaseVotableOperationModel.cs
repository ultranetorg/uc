using System.Text.Json.Serialization;

namespace Uccs.Fair;

public abstract class BaseVotableOperationModel(SiteOperation operation)
{
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
	public string? SiteId { get; set; } = operation.Site?.Id.ToString();
}
