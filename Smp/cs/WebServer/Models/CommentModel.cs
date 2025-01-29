namespace Uccs.Smp;

public class CommentModel
{
	public string Id { get; set; }

	public string Text { get; set; }

	public byte Rating { get; set; }

	public int Created { get; set; }

	// public EntityId User { get; set; }
	public string AccountId { get; set; }
	public string AccountTitle { get; set; }
}
