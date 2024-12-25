using Explorer.BLL.Models.Search;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;
using Explorer.WebApi.Models.Responses.Search;

namespace Explorer.WebApi.Controllers;

public class SearchController
(
	ILogger<AuctionsController> logger,
	IMapper mapper,
	IQueryValidator queryValidator,
	ISearchService searchService
) : BaseController
{
	private readonly ILogger<AuctionsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IQueryValidator _queryValidator = queryValidator;
	private readonly ISearchService _searchService = searchService;

	[HttpGet]
	public async Task<IEnumerable<BaseSearchResponse>> Query([FromQuery] string query, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Query)} method called ({nameof(query)}={query}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		_queryValidator.Validate(query);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultPageSize);
		PaginatedResult<BaseSearchModel> result =
			await _searchService.QueryAsync(query, paginationParams, cancellationToken);
		IEnumerable<BaseSearchResponse> items = _mapper.Map<IEnumerable<BaseSearchResponse>>(result.Items);

		return this.OkPaged(items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
