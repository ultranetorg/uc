using Explorer.BLL.Models.Auctions;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;

namespace Explorer.WebApi.Controllers;

public class AuctionsController
(
	ILogger<AuctionsController> logger,
	IMapper mapper,
	IAuctionNameValidator auctionNameValidator,
	IAuctionsService auctionsService
) : BaseController
{
	private readonly ILogger<AuctionsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IAuctionNameValidator _auctionNameValidator = auctionNameValidator;
	private readonly IAuctionsService _auctionsService = auctionsService;

	[HttpGet]
	public async Task<AuctionsModel> GetAll([FromQuery] PaginatedRequest paginatedRequest, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetAll)} method called ({{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultPageSize);
		return await _auctionsService.GetAsync(paginationParams, cancellationToken);
	}

	[HttpGet("{name}")]
	public async Task<AuctionDetailsModel> Get(string name, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(name)}={name})");

		_auctionNameValidator.Validate(name);

		return await _auctionsService.GetDetailsAsync(name, cancellationToken);
	}

	[HttpGet("{name}/bids")]
	public async Task<IEnumerable<AuctionDetailsBidModel>> GetBids(string name, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetBids)} method called ({nameof(name)}={name}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		_auctionNameValidator.Validate(name);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		PaginatedResult<AuctionDetailsBidModel> result = await _auctionsService.GetBidsAsync(name, paginationParams, cancellationToken);

		return this.OkPaged(result.Items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
