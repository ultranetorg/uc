using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Site site, Proposal proposal) =>
		site.ApprovalPolicies[proposal.OptionClass] != ApprovalPolicy.ElectedByAuthorsMajority;

	public static bool IsReviewOperation(Proposal proposal) => proposal.Options[0].Operation is ReviewCreation or ReviewEdit;
}
