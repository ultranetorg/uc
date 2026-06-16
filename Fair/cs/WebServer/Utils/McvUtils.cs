namespace Uccs.Fair;

public static class McvUtils
{
	public static IEnumerable<UserModel> LoadUsers(FairMcv mcv, AutoId[] usersIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];
		
		List<UserModel> result = new(usersIds.Length);
		
		foreach(AutoId id in usersIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;
			
			FairUser user = (FairUser) mcv.Users.Latest(id);
			UserModel model = new(user);
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