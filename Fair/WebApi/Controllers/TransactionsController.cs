using Explorer.BLL.Models.Operations;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;
using Explorer.WebApi.Models.Responses.Operations;

namespace Explorer.WebApi.Controllers;

public class TransactionsController
(
	ILogger<RoundsController> logger,
	IMapper mapper,
	ITransactionsService transactionsService,
	ITransactionIdValidator transactionIdValidator
) : BaseController
{
	private readonly ILogger<RoundsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly ITransactionsService _transactionsService = transactionsService;
	private readonly ITransactionIdValidator _transactionIdValidator = transactionIdValidator;

	[HttpGet("{id}")]
	public async Task<TransactionResponse> Get(string id, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(id)}={id})");

		_transactionIdValidator.Validate(id);

		TransactionModel model = await _transactionsService.GetAsync(id, cancellationToken);

		return _mapper.Map<TransactionResponse>(model);
	}

	[HttpGet("{id}/operations")]
	public async Task<IEnumerable<BaseOperationResponse>> GetTransactions(string id, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called ({nameof(id)}={id}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		_transactionIdValidator.Validate(id);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		PaginatedResult<BaseOperationModel> result = await _transactionsService.GetOperationsAsync(id, paginationParams, cancellationToken);
		IEnumerable<BaseOperationResponse> items = _mapper.Map<IEnumerable<BaseOperationResponse>>(result.Items);

		return this.OkPaged(items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
