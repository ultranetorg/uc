using System.Diagnostics.CodeAnalysis;

namespace Uccs.Fair;

public interface IUsersService
{
	UserModel Get([NotNull] string userId);
}
