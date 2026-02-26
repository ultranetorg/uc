namespace Uccs.Fair;

public static class McvUtils
{
	public static IEnumerable<AccountBaseModel> LoadAccounts(Mcv mcv, AutoId[] accountsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];
		
		List<AccountBaseModel> result = new(accountsIds.Length);
		
		foreach(AutoId id in accountsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;
			
			FairUser account = (FairUser) mcv.Users.Latest(id);
			AccountBaseModel model = new(account);
			result.Add(model);
		}

		return result;
	}

	public static IEnumerable<AuthorBaseAvatarModel> LoadAuthors(FairMcv mcv, AutoId[] authorsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];

		List<AuthorBaseAvatarModel> result = new(authorsIds.Length);

		foreach(AutoId id in authorsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;

			Author author = mcv.Authors.Latest(id);
			AuthorBaseAvatarModel model = new(author);
			result.Add(model);
		}

		return result;
	}
}