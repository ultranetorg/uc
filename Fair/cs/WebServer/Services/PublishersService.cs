using System.Diagnostics.CodeAnalysis;
using Uccs.Web.Pagination;

namespace Uccs.Fair;

public class PublishersService
(
#if DEBUG
	ILogger<PublishersService> logger,
#endif
	FairMcv mcv
)
{
	public TotalItemsResult<PublisherModel> GetPublishers([NotEmpty][NotNull] string storeId, [NonNegativeValue] int page, [NonNegativeValue][NonZeroValue] int pageSize, string? search, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}, {Page}, {PageSize}, {Search}", nameof(PublishersService), nameof(PublishersService.GetPublishers), storeId, page, pageSize, search);
#endif

		AutoId id = AutoId.Parse(storeId);
		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		if(!string.IsNullOrEmpty(search) && AutoId.TryParse(search, out AutoId parsedId))
		{
			Publisher publisher = store.Publishers.FirstOrDefault(x => x.Author == parsedId);
			if(publisher == null)
			{
				return TotalItemsResult<PublisherModel>.Empty;
			}

			Author author = mcv.Authors.Latest(publisher.Author);
			var model = new PublisherModel(author, publisher);
			return new TotalItemsResult<PublisherModel> {Items = [model], TotalItems = store.Publishers.Length};
		}

		return LoadPublishers(store.Publishers, page, pageSize, search, cancellationToken);
	}

	TotalItemsResult<PublisherModel> LoadPublishers(IEnumerable<Publisher> publishers, int page, int pageSize, string search, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return TotalItemsResult<PublisherModel>.Empty;

		List<PublisherModel> items = new(pageSize);
		int totalItems = 0;

		foreach(Publisher publisher in publishers)
		{
			if(cancellationToken.IsCancellationRequested)
				return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };

			Author author = mcv.Authors.Latest(publisher.Author);
			if (!SearchUtils.IsMatch(author, search))
				continue;

			if(totalItems >= page * pageSize && totalItems < (page + 1) * pageSize)
			{
				var model = new PublisherModel(author, publisher);
				items.Add(model);
			}

			++totalItems;
		}

		return new TotalItemsResult<PublisherModel> { Items = items, TotalItems = totalItems };
	}
}
