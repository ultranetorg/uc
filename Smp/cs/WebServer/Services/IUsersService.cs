using System.Diagnostics.CodeAnalysis;

namespace Uccs.Smp;

public interface IUsersService
{
	UserModel Find([NotNull] string userId);
}
