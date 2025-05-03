using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class PublicationAuthorModel(Publication publication, Product product)
	: PublicationBaseModel(publication, product)
{
}
