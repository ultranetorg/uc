namespace Uccs.Fair;

public class SiteTextChangeModel(SiteTextChange operation) : BaseVotableOperationModel(operation)
{
	public string Title { get; set; } = operation.Title;
	public string Slogan { get; set; } = operation.Slogan;
	public string Description { get; set; } = operation.Description;
}
