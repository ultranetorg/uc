using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class FilesService
(
#if DEBUG
	ILogger<CategoriesService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<FileModel> GetAuthorFiles([NotNull][NotEmpty] string storeId, [NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentException.ThrowIfNullOrEmpty(authorId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {AuthorId}, {Page}, {PageSize}", nameof(FilesService), nameof(FilesService.GetAuthorFiles), storeId, authorId, page, pageSize);
#endif

		AutoId storeEntityId = AutoId.Parse(storeId);
		AutoId authorEntityId = AutoId.Parse(authorId);

		Store store = mcv.Stores.Latest(storeEntityId);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		Author author = mcv.Authors.Latest(authorEntityId);
		if(author == null)
		{
			throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
		}

		return LoadFilesNotOptimized(author.Files, page, pageSize, cancellationToken);
	}

	public TotalItemsResult<FileModel> GetStoreFiles([NotNull][NotEmpty] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(storeId);
		ArgumentOutOfRangeException.ThrowIfNegative(page);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}", nameof(FilesService), nameof(FilesService.GetStoreFiles), storeId, page, pageSize);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadFilesNotOptimized(store.Files, page, pageSize, cancellationToken);
	}

	TotalItemsResult<FileModel> LoadFilesNotOptimized(IEnumerable<AutoId> filesIds, int page, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<FileModel>.Empty;

		int totalItems = 0;
		var result = new List<FileModel>(pageSize);
		foreach(AutoId fileId in filesIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<FileModel>() { Items = result, TotalItems = totalItems };

			File file = mcv.Files.Latest(fileId);
			if(file.Deleted)
			{
				continue;
			}

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				var model = new FileModel
				{
					Id = file.Id.ToString(),
					Refs = file.Refs
				};
				result.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<FileModel>
		{
			Items = result,
			TotalItems = totalItems
		};
	}

	public FileContentResult GetFile([NotNull][NotEmpty] string fileId)
	{
#if DEBUG
		ArgumentException.ThrowIfNullOrEmpty(fileId);

		logger.LogDebug("{ClassName}.{MethodName} method called with {FileId}", nameof(FilesService), nameof(FilesService.GetFile), fileId);
#endif

		AutoId id = AutoId.Parse(fileId);

		File file = mcv.Files.Latest(id);
		if(file == null)
		{
			throw new EntityNotFoundException(nameof(Field).ToLower(), fileId);
		}

		string mimeType = GetMimeType(file.Mime);
		return new FileContentResult(file.Data, mimeType);
	}

	string GetMimeType(FairMime mimeType)
	{
		switch (mimeType)
		{
			case FairMime.ImageJpg: return MediaTypeNames.Image.Jpeg;
			case FairMime.ImagePng: return MediaTypeNames.Image.Png;
			default: return MediaTypeNames.Application.Octet;
		}
	}
}
