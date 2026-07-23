using System.Runtime.CompilerServices;
using Uccs.Net;

namespace Uccs.Fair;

public static class ProposalUtils
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsDiscussion(Store store, Proposal proposal) => store.IsDiscussion(proposal.OptionClass); /// TODO: remove this method

	public static bool IsPublicationOperation(Proposal proposal) => proposal.Options[0].Operation
		is PublicationCreation or PublicationDeletion or PublicationPublish or PublicationUpdation or PublicationUnpublish;

	public static bool IsReviewOperation(Proposal proposal) => proposal.Options[0].Operation is ReviewCreation or ReviewEdit;

	public static bool IsUserRegistrationOperation(Proposal proposal) => proposal.Options[0].Operation is UserRegistration;

	public static bool IsUserUnregistrationOperation(Proposal proposal) => proposal.Options[0].Operation is UserUnregistration;

	public static bool IsModeratorOperation(Proposal proposal) => proposal.Options[0].Operation is StoreModeratorAddition or StoreModeratorRemoval;

	public static bool IsPublisherOperation(Proposal proposal) => proposal.Options[0].Operation is StoreAuthorsRemoval;

	public static BaseVotableOperationModel ToBaseVotableOperationModel(FairMcv mcv, StoreOperation proposal)
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
			PublicationUnpublish operation => CreatePublicationUnpublishModel(mcv, operation),
			ReviewEdit operation => new ReviewEditModel(operation),
			ReviewCreation operation => new ReviewCreationModel(operation),
			ReviewStatusChange operation => new ReviewStatusChangeModel(operation),
			StoreApprovalPolicyChange operation => new StoreApprovalPolicyChangeModel(operation),
			StoreAuthorsRemoval operation => CreateStoreAuthorsRemovalModel(mcv, operation),
			StoreAvatarChange operation => new StoreAvatarChangeModel(operation),
			StoreModeratorAddition operation => CreateStoreModeratorAdditionModel(mcv, operation),
			StoreModeratorRemoval operation => CreateStoreModeratorRemovalModel(mcv, operation),
			StoreNameChange operation => CreateStoreNameChangeModel(operation),
			StoreInfoUpdation operation => new StoreInfoUpdationModel(operation),
			UserUnregistration operation => new UserUnregistrationModel(operation),
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

	static StoreAuthorsRemovalModel CreateStoreAuthorsRemovalModel(FairMcv mcv, StoreAuthorsRemoval operation)
	{
		IEnumerable<AuthorBaseAvatarModel> removals = operation.Authors.Select(authorId =>
		{
			Author author = mcv.Authors.Latest(authorId);
			return new AuthorBaseAvatarModel(author);
		});

		return new StoreAuthorsRemovalModel(operation)
		{
			Removals = removals
		};
	}

	// TODO: split models on regular and details models. Need to avoid expensive operations with data loading.
	static StoreModeratorAdditionModel CreateStoreModeratorAdditionModel(FairMcv mcv, StoreModeratorAddition operation)
	{
		IEnumerable<UserModel> candidates = operation.Candidates.Select(candidateId =>
		{
			User user = (FairUser) mcv.Users.Latest(candidateId);
			return new UserModel(user);
		});

		return new StoreModeratorAdditionModel(operation)
		{
			Candidates = candidates,
		};
	}

	// TODO: split models on regular and details models. Need to avoid expensive operations with data loading.
	static StoreModeratorRemovalModel CreateStoreModeratorRemovalModel(FairMcv mcv, StoreModeratorRemoval operation)
	{
		User user = (FairUser) mcv.Users.Latest(operation.Moderator);
		return new StoreModeratorRemovalModel(operation)
		{
			Moderator = new UserModel(user)
		};
	}

	static StoreNameChangeModel CreateStoreNameChangeModel(StoreNameChange operation)
	{
		return new StoreNameChangeModel(operation, operation.Store.Name);
	}

	static PublicationPublishModel CreatePublicationPublishModel(FairMcv mcv, PublicationPublish operation)
	{
		Publication publication = mcv.Publications.Latest(operation.Publication);
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(operation.Category);
		return new PublicationPublishModel(operation, product, category);
	}

	static PublicationUnpublishModel CreatePublicationUnpublishModel(FairMcv mcv, PublicationUnpublish operation)
	{
		Publication publication = mcv.Publications.Latest(operation.Publication);
		Product product = mcv.Products.Latest(publication.Product);
		Category category = mcv.Categories.Latest(publication.Category);
		return new PublicationUnpublishModel(operation, product, category);
	}
}

//public static int CalculateHoursLeft(Proposal proposal, Store store)
//{
//	if (store.Policies.First(i => i.OperationClass == proposal.OptionClass).Approval == ApprovalRequirement.PublishersMajority)
//		return -1;

//	Restiction? restriction = Store.Restrictions.FirstOrDefault(x => x.OperationClass == proposal.OptionClass);
//	if(restriction == null || restriction.Flags.HasFlag(PolicyFlag.Infinite))
//		return -1;

//	return (proposal.CreationTime - Time.FromDays(30)).Hours;
//}
