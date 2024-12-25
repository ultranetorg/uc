namespace Explorer.WebApi.Models.Responses.Search;

[JsonDerivedType(typeof(AccountSearchResponse), "accountSearch")]
[JsonDerivedType(typeof(AuthorSearchResponse), "authorSearch")]
[JsonDerivedType(typeof(ResourceSearchResponse), "resourceSearch")]
public abstract class BaseSearchResponse
{
}
