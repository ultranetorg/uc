using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Site site, Proposal proposal) =>
		site.ApprovalPolicies[proposal.OptionClass] != ApprovalPolicy.ElectedByAuthorsMajority;

	public static bool IsReviewOperation(Proposal proposal)
	{
		VotableOperation operation = proposal.Options[0].Operation;

		return operation.GetType() == typeof(ReviewCreation) || operation.GetType() == typeof(ReviewEdit);
	}
}
