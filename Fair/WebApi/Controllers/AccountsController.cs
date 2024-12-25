using Explorer.BLL.Models.Operations;
using Explorer.BLL.Services.Interfaces;
using Explorer.Common.Constants;
using Explorer.WebApi.Models.Responses.Operations;

namespace Explorer.WebApi.Controllers;

public class AccountsController
(
	ILogger<AccountsController> logger,
	IMapper mapper,
	IAddressValidator addressValidator,
	IAccountsService accountsService
) : BaseController
{
	private readonly ILogger<AccountsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IAddressValidator _addressValidator = addressValidator;
	private readonly IAccountsService _accountsService = accountsService;

	[HttpGet("{address}")]
	public async Task<AccountResponse> Get(string address, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called (address={address})");

		_addressValidator.Validate(address);

		AccountModel model = await _accountsService.GetAsync(address, cancellationToken);

		return _mapper.Map<AccountResponse>(model);
	}

	[HttpGet("{address}/operations")]
	public async Task<IEnumerable<BaseOperationResponse>> GetOperations(string address, [FromQuery] PaginatedRequest paginatedRequest,
		CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(GetOperations)} method called ({nameof(address)}={address}, {{{nameof(paginatedRequest)}.{nameof(paginatedRequest.Page)}={paginatedRequest.Page}, {nameof(paginatedRequest)}.{nameof(paginatedRequest.PageSize)}={paginatedRequest.PageSize}}})");

		_addressValidator.Validate(address);

		Pagination paginationParams = new(paginatedRequest.Page, paginatedRequest.PageSize, PaginationConstants.DefaultChildItemsPageSize);
		PaginatedResult<BaseOperationModel> result =
			await _accountsService.GetOperationsAsync(address, paginationParams, cancellationToken)
			.ConfigureAwait(false);
		IEnumerable<BaseOperationResponse> items = _mapper.Map<IEnumerable<BaseOperationResponse>>(result.Items);

		return this.OkPaged(items, result.PageIndex, result.PageSize, result.TotalItems);
	}
}
