using Microsoft.AspNetCore.Mvc;

namespace Uccs.Fair;

public class FileContentResultModel(string fileId, FileContentResult fileContentResult)
{
	public string Id { get; } = fileId;

	public FileContentResult FileContentResult { get; } = fileContentResult;
}
