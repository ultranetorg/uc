using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class FilesService
(
	ILogger<CategoriesService> logger,
	FairMcv mcv
)
{
	public TotalItemsResult<string> GetAuthorFiles([NotNull][NotEmpty] string siteId, [NotNull][NotEmpty] string authorId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {SiteId}, {AuthorId}, {Page}, {PageSize}", nameof(FilesService), nameof(GetAuthorFiles), siteId, authorId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.NullOrEmpty(authorId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId siteAutoId = AutoId.Parse(siteId);
		AutoId authorAutoId = AutoId.Parse(authorId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(siteAutoId);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			Author author = mcv.Authors.Latest(authorAutoId);
			if(author == null)
			{
				throw new EntityNotFoundException(nameof(Author).ToLower(), authorId);
			}

			return LoadFilesNotOptimized(author.Files, page, pageSize, cancellationToken);
		}
	}

	public TotalItemsResult<string> GetSiteFiles([NotNull][NotEmpty] string siteId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, CancellationToken cancellationToken)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {SiteId}, {Page}, {PageSize}", nameof(FilesService), nameof(GetSiteFiles), siteId, page, pageSize);

		Guard.Against.NullOrEmpty(siteId);
		Guard.Against.Negative(page);
		Guard.Against.NegativeOrZero(pageSize);

		AutoId id = AutoId.Parse(siteId);

		lock(mcv.Lock)
		{
			Site site = mcv.Sites.Latest(id);
			if(site == null)
			{
				throw new EntityNotFoundException(nameof(Site).ToLower(), siteId);
			}

			return LoadFilesNotOptimized(site.Files, page, pageSize, cancellationToken);
		}
	}

	TotalItemsResult<string> LoadFilesNotOptimized(IEnumerable<AutoId> filesIds, int page, int pageSize, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<string>.Empty;

		int totalItems = 0;
		var result = new List<string>(pageSize);
		foreach(AutoId fileId in filesIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<string>() { Items = result, TotalItems = totalItems };

			File file = mcv.Files.Latest(fileId);
			if(file.Deleted)
			{
				continue;
			}

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				result.Add(file.Id.ToString());
			}

			++totalItems;
		}

		return new TotalItemsResult<string>
		{
			Items = result,
			TotalItems = totalItems
		};
	}

	public FileContentResult GetFile([NotNull][NotEmpty] string fileId)
	{
		logger.LogDebug("GET {ClassName}.{MethodName} method called with {FileId}", nameof(FilesService), nameof(GetFile), fileId);

		Guard.Against.NullOrEmpty(fileId);

		AutoId id = AutoId.Parse(fileId);

		lock(mcv.Lock)
		{
			File file = mcv.Files.Latest(id);
			string mimeType = GetMimeType(file.Mime);
			return new FileContentResult(file.Data, mimeType);
		}
	}

	string GetMimeType(MimeType mimeType)
	{
		switch (mimeType)
		{
			case MimeType.ImageJpg: return MediaTypeNames.Image.Jpeg;
			case MimeType.ImagePng: return MediaTypeNames.Image.Png;
			default: return MediaTypeNames.Application.Octet;
		}
	}
}
