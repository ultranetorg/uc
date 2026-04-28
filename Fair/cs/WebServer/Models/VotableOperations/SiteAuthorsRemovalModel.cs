namespace Uccs.Fair;

public class SiteAuthorsRemovalModel(SiteAuthorsRemoval operation) : BaseVotableOperationModel(operation)
{
	//public IEnumerable<string> AdditionsIds { get; set; } = operation.Additions.Select(x => x.ToString());
	public IEnumerable<string> RemovalsIds { get; set; } = operation.Authors.Select(x => x.ToString());
	public IEnumerable<AuthorBaseModel> Removals { get; init; } = null!;
}
