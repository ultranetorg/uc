using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Site site, Proposal proposal) =>
		site.ApprovalPolicies[proposal.OptionClass] != ApprovalPolicy.ElectedByAuthorsMajority;

	public static bool IsPublicationOperation(Proposal proposal) => proposal.Options[0].Operation
		is PublicationCreation or PublicationDeletion or PublicationPublish or PublicationRemoveFromChanged or PublicationUpdation;

	public static bool IsReviewOperation(Proposal proposal) => proposal.Options[0].Operation is ReviewCreation or ReviewEdit;

	public static bool IsUserOperation(Proposal proposal) => proposal.Options[0].Operation is UserDeletion or UserRegistration;

	public static BaseVotableOperationModel ToBaseVotableOperationModel(VotableOperation proposal)
	{
		return proposal switch
		{
			CategoryAvatarChange operation => new CategoryAvatarChangeModel(operation),
			CategoryCreation operation => new CategoryCreationModel(operation),
			CategoryDeletion operation => new CategoryDeletionModel(operation),
			CategoryMovement operation => new CategoryMovementModel(operation),
			CategoryTypeChange operation => new CategoryTypeChangeModel(operation),
			PublicationCreation operation => new PublicationCreationModel(operation),
			PublicationDeletion operation => new PublicationDeletionModel(operation),
			PublicationPublish operation => new PublicationPublishModel(operation),
			PublicationRemoveFromChanged operation => new PublicationRemoveFromChangedModel(operation),
			PublicationUpdation operation => new PublicationUpdationModel(operation),
			ReviewCreation operation => new ReviewCreationModel(operation),
			ReviewEdit operation => new ReviewEditModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
			SiteAvatarChange operation => new SiteAvatarChangeModel(operation),
			SiteModeratorsChange operation => new SiteModeratorsChangeModel(operation),
			SiteNicknameChange operation => new SiteNicknameChangeModel(operation),
			SitePolicyChange operation => new SitePolicyChangeModel(operation),
			SiteTextChange operation => new SiteTextChangeModel(operation),
			UserDeletion operation => new UserDeletionModel(operation),
			UserRegistration operation => new UserRegistrationModel(operation),
			_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
		};
	}

	public static IEnumerable<ProposalOptionModel> MapOptions(ProposalOption[] options)
	{
		IList<ProposalOptionModel> result = new List<ProposalOptionModel>(options.Length);

		foreach(ProposalOption option in options)
		{
			ProposalOptionModel model = new(option);
			model.Operation = ProposalUtils.ToBaseVotableOperationModel(option.Operation);

			result.Add(model);
		}

		return result;
	}
}
