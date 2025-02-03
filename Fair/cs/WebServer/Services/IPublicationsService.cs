using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IPublicationsService
{
	PublicationModel Find([NotEmpty] string publicationId);
}
