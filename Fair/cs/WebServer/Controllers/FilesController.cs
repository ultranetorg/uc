using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FilesController
(
	ILogger<FilesController> logger,
	AutoIdValidator autoIdValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet("{fileId}")]
	public FileContentResult Get(string fileId)
	{
		logger.LogInformation("GET {ControllerName}.{ActionName} method called with {FileId}", nameof(FilesController), nameof(Get), fileId);

		autoIdValidator.Validate(fileId, nameof(Uccs.Fair.File).ToLower());

		return filesService.GetFile(fileId);
	}
}