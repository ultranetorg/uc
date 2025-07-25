﻿namespace Uccs.Fair;

public class ProposalOptionModel(ProposalOption option)
{
	public string Title { get; set; } = option.Title;

	public BaseVotableOperationModel Operation { get; set; }

	public IEnumerable<AccountBaseModel> YesAccounts { get; set; } = null!;
}
