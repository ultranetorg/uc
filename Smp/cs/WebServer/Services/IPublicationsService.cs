using System.Diagnostics.CodeAnalysis;

namespace Uccs.Smp;

public interface IPublicationsService
{
	PublicationModel Find([NotEmpty] string publicationId);
}
