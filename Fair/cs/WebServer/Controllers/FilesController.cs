using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FilesController
(
	ILogger<FilesController> logger,
	IAutoIdValidator autoIdValidator,
	FileService fileService
) : BaseController
{
	[HttpGet("{fileId}")]
	public IActionResult Get(string fileId)
	{
		logger.LogInformation($"GET {nameof(FilesController)}.{nameof(Get)} method called with {{fileId}}", fileId);
		autoIdValidator.Validate(fileId, nameof(Uccs.Fair.File).ToLower());
		
		return Ok(fileService.GetFileData(fileId));
	}
}