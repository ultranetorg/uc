using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Site site, Proposal proposal) => site.IsDiscussion(proposal.OptionClass); /// TODO: remove this method

	public static bool IsPublicationOperation(Proposal proposal) => proposal.Options[0].Operation
		is PublicationCreation or PublicationDeletion or PublicationPublish or PublicationUpdation;

	public static bool IsReviewOperation(Proposal proposal) => proposal.Options[0].Operation is ReviewCreation or ReviewEdit;

	public static bool IsUserOperation(Proposal proposal) => proposal.Options[0].Operation is UserUnregistration or UserRegistration;

	public static BaseVotableOperationModel ToBaseVotableOperationModel(SiteOperation proposal)
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
			PublicationUpdation operation => new PublicationUpdationModel(operation),
			ReviewEdit operation => new ReviewEditModel(operation),
			ReviewCreation operation => new ReviewCreationModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			SiteApprovalPolicyChange operation => new SiteApprovalPolicyChangeModel(operation),
			SiteAuthorsChange operation => new SiteAuthorsChangeModel(operation),
			SiteAvatarChange operation => new SiteAvatarChangeModel(operation),
			SiteModeratorAddition operation => new SiteModeratorAdditionModel(operation),
			SiteModeratorRemoval operation => new SiteModeratorRemovalModel(operation),
			SiteNameChange operation => new SiteNicknameChangeModel(operation),
			SiteTextChange operation => new SiteTextChangeModel(operation),
			UserUnregistration operation => new UserDeletionModel(operation),
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
