using Explorer.BLL.Models.Operations;
using Explorer.BLL.Models.Search;
using Explorer.MongoDB.Documents.Operations;
using Explorer.WebApi.Models.Responses.Operations;
using Explorer.WebApi.Models.Responses.Search;
using Uccs;

using AccountResponse = Explorer.WebApi.Models.Responses.AccountResponse;

namespace Explorer.Api.Models.Mappings;

public sealed class ModelsToResponsesProfile : Profile
{
	public ModelsToResponsesProfile()
	{
		CreateBaseTypesMappings();

		CreateMap<AccountModel, AccountResponse>();

		CreateMap<TransactionModel, TransactionResponse>();

		CreateOperationsMappings();

		CreateSearchMappings();
	}

	private void CreateBaseTypesMappings()
	{
		CreateMap<byte[]?, string?>().ConvertUsing(source => source != null ? source.ToHex() : null);

		CreateMap(typeof(ChildItemsResult<>), typeof(ChildItemsResult<>));
	}

	private void CreateOperationsMappings()
	{
		CreateMap<BaseOperationModel, BaseOperationResponse>();

		CreateMap<AnalysisResultUpdationModel, AnalysisResultUpdationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<AuthorBidModel, AuthorBidResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<AuthorMigrationModel, AuthorMigrationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<AuthorRegistrationModel, AuthorRegistrationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<AuthorTransferModel, AuthorTransferResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<CandidacyDeclarationModel, CandidacyDeclarationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<EmissionModel, EmissionResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<ResourceCreationModel, ResourceCreationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<ResourceDeletionModel, ResourceDeletionResponse>()
					.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<ResourceLinkCreationModel, ResourceLinkCreationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<ResourceUpdationModel, ResourceUpdationResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();

		CreateMap<UntTransferModel, UntTransferResponse>()
			.IncludeBase<BaseOperationModel, BaseOperationResponse>();
	}

	private void CreateSearchMappings()
	{
		CreateMap<BaseSearchModel, BaseSearchResponse>();

		CreateMap<AccountSearchModel, AccountSearchResponse>()
			.IncludeBase<BaseSearchModel, BaseSearchResponse>();

		CreateMap<AuthorSearchModel, AuthorSearchResponse>()
			.IncludeBase<BaseSearchModel, BaseSearchResponse>();

		CreateMap<ResourceSearchModel, ResourceSearchResponse>()
			.IncludeBase<BaseSearchModel, BaseSearchResponse>();
	}
}
