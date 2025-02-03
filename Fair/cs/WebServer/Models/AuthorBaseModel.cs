namespace Uccs.Fair; 

public class AuthorBaseModel
{
	public string Id { get; set; }

	public string Title { get; set; }

	public int Expiration { get; set; }

	public short SpaceReserved { get; set; }
	public short SpaceUsed { get; set; }

	public AuthorBaseModel(Author author)
	{
		Id = author.Id.ToString();
		Title = author.Title;
		Expiration = author.Expiration.Days;
		SpaceReserved = author.SpaceReserved;
		SpaceUsed = author.SpaceUsed;
	}
}
