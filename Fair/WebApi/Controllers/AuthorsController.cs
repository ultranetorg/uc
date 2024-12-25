using Explorer.BLL.Models.Resource;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;

namespace Explorer.WebApi.Controllers;

public class AuthorsController
(
	ILogger<AuthorsController> logger,
	IMapper mapper,
	IAuthorNameValidator authorNameValidator,
	IAuthorsService authorsService
) : BaseController
{
	private readonly ILogger<AuthorsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IAuthorNameValidator _authorNameValidator = authorNameValidator;
	private readonly IAuthorsService _authorsService = authorsService;

	[HttpGet("{name}")]
	public async Task<AuthorModel> Get(string name, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(name)}={name})");

		_authorNameValidator.Validate(name);

		return await _authorsService.GetAsync(name, cancellationToken);
	}

	[HttpGet("{name}/resources")]
	public async Task<IEnumerable<ResourceInfoModel>> GetResources(string name, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetResources)} method called ({nameof(name)}={name}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		_authorNameValidator.Validate(name);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		PaginatedResult<ResourceInfoModel> result =
			await _authorsService.GetResourcesAsync(name, paginationParams, cancellationToken)
			.ConfigureAwait(false);

		return this.OkPaged(result.Items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
