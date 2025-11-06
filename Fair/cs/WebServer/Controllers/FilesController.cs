using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

[Route("api/files/{fileId}")]
public class FilesController
(
	ILogger<FilesController> logger,
	IAutoIdValidator autoIdValidator,
	FilesService filesService
) : BaseController
{
	[HttpGet]
	public FileContentResult Get(string fileId, CancellationToken cancellationToken)
	{
		logger.LogInformation("GET {ControllerName}.{MethodName} method called with {FileId}", nameof(FilesController), nameof(Get), fileId);

		autoIdValidator.Validate(fileId, nameof(File).ToLower());

		return filesService.GetFile(fileId);
	}
}
