using Explorer.BLL.Services.Interfaces;

namespace Explorer.WebApi.Controllers;

public class ChainController
(
	ILogger<ChainController> logger,
	IMapper mapper,
	IChainService chainService
) : BaseController
{
	private readonly ILogger<ChainController> _logger = logger;
	private readonly IMapper _mapper = mapper;
	private readonly IChainService _chainService = chainService;

	[HttpGet]
	public async Task<ChainModel> Get(CancellationToken cancellationToken)
	{
		_logger.LogInformation($"{nameof(Get)} method called");

		return await _chainService.GetChainDataAsync(cancellationToken);
	}
}
