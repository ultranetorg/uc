namespace Uccs.Fair;

public static class VotingUtils
{
	public static int CalculateVotesRequiredToWinPerpetualSurvey(int sitePublishersCount) => sitePublishersCount / 2 + (sitePublishersCount & 1);

	public static int CalculateVotesRequiredToWinProposal(ApprovalRequirement approval, Site site)
	{
		return approval switch
		{
			ApprovalRequirement.AnyModerator => 1,
			ApprovalRequirement.ModeratorsMajority => site.Moderators.Length / 2 + (site.Moderators.Length & 1),
			ApprovalRequirement.AllModerators => site.Moderators.Length,
			ApprovalRequirement.PublishersMajority => site.Publishers.Length / 2 + (site.Publishers.Length & 1),
			_ => -1
		};
	}
}
