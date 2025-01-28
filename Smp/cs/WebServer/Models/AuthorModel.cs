namespace Uccs.Smp;

public class AuthorModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public string OwnerId { get; set; }

	public int Expiration { get; set; }

	public short SpaceReserved { get; set; }
	public short SpaceUsed { get; set; }
}
