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
					///TODO new JsonDerivedType(typeof(NicknameChangeModel), "nickname-change"),
					//new JsonDerivedType(typeof(PublicationApprovalModel), "publication-approval"),
					new JsonDerivedType(typeof(PublicationPublish), "publication-publish-change"),
					new JsonDerivedType(typeof(PublicationUpdationModel), "publication-updation"),
					new JsonDerivedType(typeof(ReviewStatusChangeModel), "review-status-change"),
					new JsonDerivedType(typeof(ReviewTextModerationModel), "review-text-moderation"),
					new JsonDerivedType(typeof(SiteAuthorsChangeModel), "site-authors-change"),
					new JsonDerivedType(typeof(SiteDescriptionChangeModel), "site-description-change"),
					new JsonDerivedType(typeof(SiteModeratorsChangeModel), "site-moderators-change"),
					new JsonDerivedType(typeof(SitePolicyChangeModel), "site-policy-change"),
				},
			};
		}

		return jsonTypeInfo;
	}
}
