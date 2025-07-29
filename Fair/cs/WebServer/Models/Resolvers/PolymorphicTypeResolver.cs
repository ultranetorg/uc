using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Uccs.Fair;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
	public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);
		if (jsonTypeInfo.Type == typeof(BaseVotableOperationModel))
		{
			jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
			{
				DerivedTypes =
				{
					new JsonDerivedType(typeof(CategoryAvatarChangeModel), "category-avatar-change"),
					new JsonDerivedType(typeof(CategoryCreationModel), "category-creation"),
					new JsonDerivedType(typeof(CategoryDeletionModel), "category-deletion"),
					new JsonDerivedType(typeof(CategoryMovementModel), "category-movement"),
					new JsonDerivedType(typeof(CategoryTypeChangeModel), "category-type-change"),
					new JsonDerivedType(typeof(PublicationCreationModel), "publication-creation"),
					new JsonDerivedType(typeof(PublicationDeletionModel), "publication-deletion"),
					new JsonDerivedType(typeof(PublicationPublishModel), "publication-publish"),
					new JsonDerivedType(typeof(PublicationRemoveFromChangedModel), "publication-remove-from-changed"),
					new JsonDerivedType(typeof(PublicationUpdationModel), "publication-updation"),
					new JsonDerivedType(typeof(ReviewCreationModel), "review-creation"),
					new JsonDerivedType(typeof(ReviewEditModerationModel), "review-edit-moderation"),
					new JsonDerivedType(typeof(ReviewStatusChangeModel), "review-status-change"),
					new JsonDerivedType(typeof(SiteAuthorsChangeModel), "site-authors-change"),
					new JsonDerivedType(typeof(SiteAvatarChangeModel), "site-avatar-change"),
					new JsonDerivedType(typeof(SiteModeratorsChangeModel), "site-moderators-change"),
					new JsonDerivedType(typeof(SiteNicknameChangeModel), "site-nickname-change"),
					new JsonDerivedType(typeof(SitePolicyChangeModel), "site-policy-change"),
					new JsonDerivedType(typeof(SiteTextChangeModel), "site-text-change"),
					new JsonDerivedType(typeof(UserDeletionModel), "user-deletion"),
					new JsonDerivedType(typeof(UserRegistrationModel), "user-registration"),
				},
			};
		}

		return jsonTypeInfo;
	}
}
