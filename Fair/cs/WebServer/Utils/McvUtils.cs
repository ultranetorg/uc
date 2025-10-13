﻿namespace Uccs.Fair;

public static class McvUtils
{
	public static IEnumerable<AccountBaseModel> LoadAccounts(Mcv mcv, AutoId[] accountsIds, CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
			return [];
		
		List<AccountBaseModel> result = new(accountsIds.Length);
		
		foreach(AutoId moderatorsId in accountsIds)
		{
			if(cancellationToken.IsCancellationRequested)
				return result;
			
			FairAccount account = (FairAccount) mcv.Accounts.Latest(moderatorsId);
			result.Add(new AccountBaseModel(account));
		}

		return result;
	}
}