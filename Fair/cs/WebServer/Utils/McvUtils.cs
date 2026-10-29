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

	public static CreatorModel LoadCreator(FairMcv mcv, Proposal proposal) => LoadCreator(mcv, proposal.By, proposal.As);

	public static CreatorModel LoadCreator(FairMcv mcv, AutoId id, Role role)
	{
		switch(role)
		{
			case Role.Moderator:
			case Role.User:
			{
				FairUser user = (FairUser) mcv.Users.Latest(id);
				return new CreatorModel {Id = id, Title = user.Name, As = role, AvatarId = id};
			}
			case Role.Publisher:
			{
				Author author = mcv.Authors.Latest(id);
				return new CreatorModel {Id = id, Title = author.Title, As = role, AvatarId = author.Avatar};
			}
			default:
			{
				throw new InvalidOperationException($"Unexpected creator role: {role}");	
			}
				
		}
	}
}