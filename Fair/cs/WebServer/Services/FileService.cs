using Ardalis.GuardClauses;

namespace Uccs.Fair;

public class FileService(
	ILogger<FileService> logger,
	FairMcv mcv)
{
	public byte[] GetFileData(string fileId)
	{
		logger.LogDebug($"GET {nameof(FileService)}.{nameof(FileService.GetFileData)} method called with {{fileId}}",
			fileId);

		Guard.Against.NullOrEmpty(fileId);

		AutoId id = AutoId.Parse(fileId);

		lock(mcv.Lock)
		{
			File file = mcv.Files.Latest(id);
			if(file == null)
			{
				throw new EntityNotFoundException(nameof(File).ToLower(), fileId);
			}

			return file.Data;
		}
	}
}