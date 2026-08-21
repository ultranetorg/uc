using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FilesController
(
#if DEBUG
	ILogger<FilesController> logger,
#endif
	FilesService filesService
) : BaseController
{
	[HttpGet("{fileId}")]
	public FileContentResult Get(string fileId)
	{
#if DEBUG
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {FileId}", nameof(FilesController), nameof(FilesController.Get), fileId);
#endif

		AutoIdValidator.Validate(fileId, nameof(Uccs.Fair.File).ToLower());

		return filesService.GetFile(fileId);
	}
}