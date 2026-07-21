namespace Uccs.Fair;

public static class VotingUtils
{
	public static int CalculateVotesRequiredToWinPerpetualSurvey(int storePublishersCount) => storePublishersCount / 2 + (storePublishersCount & 1);

	public static int CalculateVotesRequiredToWinProposal(ApprovalRequirement approval, Store store)
	{
		return approval switch
		{
			ApprovalRequirement.AnyModerator => 1,
			ApprovalRequirement.ModeratorsMajority => store.Moderators.Length / 2 + (store.Moderators.Length & 1),
			ApprovalRequirement.AllModerators => store.Moderators.Length,
			ApprovalRequirement.PublishersMajority => store.Publishers.Length / 2 + (store.Publishers.Length & 1),
			_ => -1
		};
	}
}
