using Explorer.BLL.Models.Operations;
using Explorer.BLL.Services.Interfaces;
using Explorer.WebApi.Models.Responses.Operations;

namespace Explorer.WebApi.Controllers;

public class OperationsController
(
	ILogger<OperationsController> logger,
	IMapper mapper,
	IOperationsService operationsService,
	IOperationIdValidator operationIdValidator
) : BaseController
{
	private readonly ILogger<OperationsController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IOperationsService _operationsService = operationsService;
	private readonly IOperationIdValidator _operationIdValidator = operationIdValidator;

	[HttpGet("{id}")]
	public async Task<BaseOperationResponse> Get(string id, CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called with {nameof(id)}={id}");

		_operationIdValidator.Validate(id);

		BaseOperationModel model = await _operationsService.GetAsync(id, cancellationToken);

		return _mapper.Map<BaseOperationModel, BaseOperationResponse>(model);
	}
}
