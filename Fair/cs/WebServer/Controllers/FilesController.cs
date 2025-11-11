using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FilesController
(
	ILogger<FilesController> logger,
	IAutoIdValidator autoIdValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet("{fileId}")]
	public IActionResult Get(string fileId)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {FileId}", nameof(FilesController), nameof(Get), fileId);
		autoIdValidator.Validate(fileId, nameof(Uccs.Fair.File).ToLower());
		
		return filesService.GetFile(fileId);
	}
}