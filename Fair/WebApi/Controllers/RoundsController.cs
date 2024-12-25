using Explorer.BLL.Models.Round;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;

namespace Explorer.WebApi.Controllers;

public class RoundsController
(
	ILogger<RoundsController> logger,
	IMapper mapper,
	IRoundsService roundsService
) : BaseController
{
	private readonly ILogger<RoundsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IRoundsService _roundsService = roundsService;

	[HttpGet("{id}")]
	public async Task<RoundModel> Get(int id, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(id)}={id})");

		If.Value(id).IsNegative().Throw(() => new InvalidRoundIdException(id));

		return await _roundsService.GetAsync(id, cancellationToken);
	}

	[HttpGet("{id}/transactions")]
	public async Task<IEnumerable<RoundTransactionModel>> GetTransactions(int id, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(id)}={id}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		If.Value(id).IsNegative().Throw(() => new InvalidRoundIdException(id));

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		PaginatedResult<RoundTransactionModel> result = await _roundsService.GetTransactionsAsync(id, paginationParams, cancellationToken);
		IEnumerable<RoundTransactionModel> items = _mapper.Map<IEnumerable<RoundTransactionModel>>(result.Items);

		return this.OkPaged(items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
