using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public class ModeratorsService
(
#if DEBUG
	ILogger<ModeratorsService> logger,
#endif
	FairMcv mcv
)
{
	public IEnumerable<ModeratorModel> GetModerators([NotEmpty] string storeId, CancellationToken cancellationToken)
	{
		ArgumentException.ThrowIfNullOrEmpty(storeId);

#if DEBUG
		logger.LogDebug("{ClassName}.{MethodName} method called with {StoreId}", nameof(ModeratorsService), nameof(ModeratorsService.GetModerators), storeId);
#endif

		AutoId id = AutoId.Parse(storeId);

		Store store = mcv.Stores.Latest(id);
		if(store == null)
		{
			throw new EntityNotFoundException(nameof(Store).ToLower(), storeId);
		}

		return LoadModerators(store.Moderators, cancellationToken);
	}

	public IEnumerable<ModeratorModel> LoadModerators(Moderator[] moderators, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<ModeratorModel> result = new(moderators.Length);

		foreach(Moderator moderator in moderators)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			FairUser account = (FairUser) mcv.Users.Latest(moderator.User);
			var model = new ModeratorModel(account, moderator.BannedTill);
			result.Add(model);
		}

		return result;
	}
}
