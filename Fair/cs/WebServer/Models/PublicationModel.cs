namespace Uccs.Fair;

public class PublicationModel(Publication publication, Product product)
	: PublicationBaseModel(publication, product)
{
	// TODO: fix.
	public byte AverageRating { get; set; }
}