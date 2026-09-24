using Uccs.Net;

namespace Uccs.Fair;

public class StoreModeratorAdditionModel(StoreModeratorAddition operation) : BaseVotableOperationModel(operation)
{
	public IEnumerable<AutoId> CandidatesIds { get; set; } = operation.Candidates;
	public IEnumerable<UserModel> Candidates { get; init; } = null!;
}