using Explorer.BLL.Models.Resource;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;
using System.Net;

namespace Explorer.WebApi.Controllers;

[Route("api/authors/")]
public class ResourcesController
(
	ILogger<ResourcesController> logger,
	IMapper mapper,
	IResourcesService resourcesService,
	IAuthorNameValidator authorNameValidator,
	IResourceNameValidator resourceNameValidator,
	IResourceIdValidator resourceIdValidator
) : BaseController
{
	private readonly ILogger<ResourcesController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IResourcesService _resourcesService = resourcesService;
	private readonly IAuthorNameValidator _authorNameValidator = authorNameValidator;
	private readonly IResourceNameValidator _resourceNameValidator = resourceNameValidator;
	private readonly IResourceIdValidator _resourceIdValidator = resourceIdValidator;

	[HttpGet("{author}/resources/{**name}")]
	public async Task<ResourceModel> Get(string author, string name, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(author)}={author}, {nameof(name)}={name})");

		string decodedName = WebUtility.UrlDecode(name);
		_authorNameValidator.Validate(author);
		_resourceNameValidator.Validate(name);

		return await _resourcesService.GetAsync(author, decodedName, cancellationToken);
	}

	[HttpGet("{author}/resources/{name}/outbound_links")]
	public async Task<IEnumerable<ResourceLinkModel>> GetLinks(string author, string name,
		[FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetLinks)} method called ({nameof(author)}={author}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		string decodedName = WebUtility.UrlDecode(name);
		_authorNameValidator.Validate(author);
		_resourceNameValidator.Validate(name);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		ChildItemsResult<ResourceLinkModel> result = await _resourcesService.GetOutboundsAsync(author, decodedName, paginationParams, cancellationToken);

		return this.OkPaged(result.Items, paginationParams.Page, paginationParams.PageSize, result.TotalItems);
	}

	[HttpGet("{author}/resources/{name}/inbound_links")]
	public async Task<IEnumerable<ResourceLinkModel>> GetInboundLinks(string author, string name,
		[FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetInboundLinks)} method called ({nameof(author)}={author}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		string decodedName = WebUtility.UrlDecode(name);
		_authorNameValidator.Validate(author);
		_resourceNameValidator.Validate(name);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		ChildItemsResult<ResourceLinkModel> result = await _resourcesService.GetInboundsAsync(author, decodedName, paginationParams, cancellationToken);

		return this.OkPaged(result.Items, paginationParams.Page, paginationParams.PageSize, result.TotalItems);
	}
}
