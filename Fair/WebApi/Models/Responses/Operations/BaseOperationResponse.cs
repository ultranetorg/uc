namespace Explorer.WebApi.Models.Responses.Operations;

[JsonDerivedType(typeof(AnalysisResultUpdationResponse), "analysisResultUpdation")]
[JsonDerivedType(typeof(AuthorBidResponse), "authorBid")]
[JsonDerivedType(typeof(AuthorMigrationResponse), "authorMigration")]
[JsonDerivedType(typeof(AuthorRegistrationResponse), "authorRegistration")]
[JsonDerivedType(typeof(AuthorTransferResponse), "authorTransfer")]
[JsonDerivedType(typeof(CandidacyDeclarationResponse), "candidacyDeclaration")]
[JsonDerivedType(typeof(EmissionResponse), "emission")]
[JsonDerivedType(typeof(ResourceCreationResponse), "resourceCreation")]
[JsonDerivedType(typeof(ResourceDeletionResponse), "resourceDeletion")]
[JsonDerivedType(typeof(ResourceLinkCreationResponse), "resourceLinkCreation")]
[JsonDerivedType(typeof(ResourceLinkDeletionResponse), "resourceLinkDeletion")]
[JsonDerivedType(typeof(ResourceUpdationResponse), "resourceUpdation")]
[JsonDerivedType(typeof(UntTransferResponse), "untTransfer")]
public class BaseOperationResponse : BaseRateResponse
{
	[JsonPropertyOrder(-3)]
	public string Id { get; set; } = null!;

	[JsonPropertyOrder(-2)]
	public string Signer { get; set; } = null!;

	[JsonPropertyOrder(-1)]
	public string TransactionId { get; set; } = null!;
}
