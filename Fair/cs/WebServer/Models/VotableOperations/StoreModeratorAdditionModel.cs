using Uccs.Net;

namespace Uccs.Fair;

public class StoreModeratorAdditionModel(StoreModeratorAddition operation) : BaseVotableOperationModel(operation)
{
	public IEnumerable<string> CandidatesIds { get; set; } = operation.Candidates.Select(x => x.ToString());
	public IEnumerable<UserModel> Candidates { get; init; } = null!;
}