using System.Runtime.CompilerServices;
using Uccs.Net;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Site site, Proposal proposal) => site.IsDiscussion(proposal.OptionClass); /// TODO: remove this method

	public static bool IsPublicationOperation(Proposal proposal) => proposal.Options[0].Operation
		is PublicationCreation or PublicationDeletion or PublicationPublish or PublicationUpdation or PublicationUnpublish;

	public static bool IsReviewOperation(Proposal proposal) => proposal.Options[0].Operation is ReviewCreation or ReviewEdit;

	public static bool IsUserOperation(Proposal proposal) => proposal.Options[0].Operation is UserUnregistration or UserRegistration;

	public static bool IsModeratorOperation(Proposal proposal) => proposal.Options[0].Operation is SiteModeratorAddition or SiteModeratorRemoval;

	public static bool IsPublisherOperation(Proposal proposal) => proposal.Options[0].Operation is SiteAuthorsRemoval;

	public static BaseVotableOperationModel ToBaseVotableOperationModel(FairMcv mcv, SiteOperation proposal)
	{
		return proposal switch
		{
			CategoryAvatarChange operation => CreateCategoryAvatarChangeModel(mcv, operation),
			CategoryCreation operation => CreateCategoryCreationModel(mcv, operation),
			CategoryDeletion operation => CreateCategoryDeletionModel(mcv, operation),
			CategoryMovement operation => CreateCategoryMovementModel(mcv, operation),
			CategoryTypeChange operation => CreateCategoryTypeChangeModel(mcv, operation),
			PublicationPublish operation => CreatePublicationPublishModel(mcv, operation),
			PublicationCreation operation => new PublicationCreationModel(operation),
			PublicationDeletion operation => new PublicationDeletionModel(operation),
			PublicationUpdation operation => new PublicationUpdationModel(operation),
			ReviewEdit operation => new ReviewEditModel(operation),
			ReviewCreation operation => new ReviewCreationModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			SiteApprovalPolicyChange operation => new SiteApprovalPolicyChangeModel(operation),
			SiteAuthorsRemoval operation => CreateSiteAuthorsRemovalModel(mcv, operation),
			SiteAvatarChange operation => new SiteAvatarChangeModel(operation),
			SiteModeratorAddition operation => CreateSiteModeratorAdditionModel(mcv, operation),
			SiteModeratorRemoval operation => CreateSiteModeratorRemovalModel(mcv, operation),
			SiteNameChange operation => CreateSiteNameChangeModel(operation),
			SiteTextChange operation => new SiteTextChangeModel(operation),
			UserUnregistration operation => new UserDeletionModel(operation),
			UserRegistration operation => new UserRegistrationModel(operation),
			_ => throw new NotSupportedException($"Operation type {proposal.GetType()} is not supported")
		};
	}

	static CategoryAvatarChangeModel CreateCategoryAvatarChangeModel(FairMcv mcv, CategoryAvatarChange operation)
	{
		Category category = mcv.Categories.Latest(operation.Category);
		return new CategoryAvatarChangeModel(operation, category);
	}

	static CategoryCreationModel CreateCategoryCreationModel(FairMcv mcv, CategoryCreation operation)
	{
		Category? category = operation.Parent != null ? mcv.Categories.Latest(operation.Parent) : null;
		return new CategoryCreationModel(operation, category);
	}

	static CategoryDeletionModel CreateCategoryDeletionModel(FairMcv mcv, CategoryDeletion operation)
	{
		Category category = mcv.Categories.Latest(operation.Category);
		return new CategoryDeletionModel(operation, category);
	}

	static CategoryMovementModel CreateCategoryMovementModel(FairMcv mcv, CategoryMovement operation)
	{
		Category category = mcv.Categories.Latest(operation.Category);
		Category? parentCategory = operation.Parent != null ? mcv.Categories.Latest(operation.Parent) : null;
		return new CategoryMovementModel(operation, category, parentCategory);
	}

	static CategoryTypeChangeModel CreateCategoryTypeChangeModel(FairMcv mcv, CategoryTypeChange operation)
	{
		Category category = mcv.Categories.Latest(operation.Category);
		return new CategoryTypeChangeModel(operation, category);
	}

	static SiteAuthorsRemovalModel CreateSiteAuthorsRemovalModel(FairMcv mcv, SiteAuthorsRemoval operation)
	{
		IEnumerable<AuthorBaseModel> removals = operation.Authors.Select(authorId =>
		{
			Author author = mcv.Authors.Latest(authorId);
			return new AuthorBaseModel(author);
		});

		return new SiteAuthorsRemovalModel(operation)
		{
			Removals = removals
		};
	}

	// TODO: split models on regular and details models. Need to avoid expensive operations with data loading.
	static SiteModeratorAdditionModel CreateSiteModeratorAdditionModel(FairMcv mcv, SiteModeratorAddition operation)
	{
		IEnumerable<UserModel> candidates = operation.Candidates.Select(candidateId =>
		{
			User user = (FairUser) mcv.Users.Latest(candidateId);
			return new UserModel(user);
		});

		return new SiteModeratorAdditionModel(operation)
		{
			Candidates = candidates,
		};
	}

	// TODO: split models on regular and details models. Need to avoid expensive operations with data loading.
	static SiteModeratorRemovalModel CreateSiteModeratorRemovalModel(FairMcv mcv, SiteModeratorRemoval operation)
	{
		User user = (FairUser) mcv.Users.Latest(operation.Moderator);
		return new SiteModeratorRemovalModel(operation)
		{
			Moderator = new UserModel(user)
		};
	}

	static SiteNameChangeModel CreateSiteNameChangeModel(SiteNameChange operation)
	{
		return new SiteNameChangeModel(operation, operation.Site.Name);
	}

	static PublicationPublishModel CreatePublicationPublishModel(FairMcv mcv, PublicationPublish operation)
	{
		Publication publication = mcv.Publications.Latest(operation.Publication);
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(operation.Category);
		return new PublicationPublishModel(operation, product, category);
	}

	public static int CalculateHoursLeft(Proposal proposal, Site site)
	{
		if (site.Policies.First(i => i.OperationClass == proposal.OptionClass).Approval == ApprovalRequirement.PublishersMajority)
			return -1;
		if (Site.Restrictions.First(i => i.OperationClass == proposal.OptionClass).Flags.HasFlag(PolicyFlag.Infinite))
			return -1;
		return (proposal.CreationTime - Time.FromDays(30)).Hours;
	}
}
