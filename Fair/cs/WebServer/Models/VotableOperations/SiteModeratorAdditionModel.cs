using Uccs.Net;

namespace Uccs.Fair;

public class SiteModeratorAdditionModel(SiteModeratorAddition operation) : BaseVotableOperationModel(operation)
{
	public IEnumerable<string> CandidatesIds { get; set; } = operation.Candidates.Select(x => x.ToString());
}