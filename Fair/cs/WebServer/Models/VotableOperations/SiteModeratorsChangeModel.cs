using Uccs.Net;

namespace Uccs.Fair;

public class SiteModeratorsChangeModel(SiteModeratorsChange operation) : BaseVotableOperationModel(operation)
{
	public IEnumerable<string> AdditionsIds { get; set; } = operation.Additions.Select(x => x.ToString());
	public IEnumerable<string> RemovalsIds { get; set; } = operation.Removals.Select(x => x.ToString());
}
