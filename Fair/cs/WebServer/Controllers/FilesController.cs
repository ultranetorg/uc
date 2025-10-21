using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FilesController
(
	ILogger<FilesController> logger,
	IAutoIdValidator autoIdValidator,
	FileService fileService
) : BaseController
{
	[HttpGet("{id}")]
	public IActionResult Get(string id)
	{
		logger.LogInformation($"GET {nameof(FilesController)}.{nameof(Get)} method called with {{id}}", id);
		autoIdValidator.Validate(id, nameof(Product).ToLower());
		
		return Ok(fileService.GetFileData(id));
	}
}