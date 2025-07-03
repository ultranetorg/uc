using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IAccountsService
{
	UserModel Get([NotNull] string userId);
}
