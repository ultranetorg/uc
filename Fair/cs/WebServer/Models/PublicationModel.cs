namespace Uccs.Fair;

public class PublicationModel(Publication publication, Product product)
	: PublicationBaseModel(publication, product)
{
	public string[] SupportedOSes { get; set; }

	// TODO: fix.
	public byte AverageRating { get; set; }
}