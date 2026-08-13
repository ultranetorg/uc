using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class UserNameValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string name)
	{
		if(name.Length < ValidationConstants.UserNameMinLength || name.Length > ValidationConstants.UserNameMaxLength || !Regexes.UserName.IsMatch(name))
		{
			throw new InvalidEntityParameterException(nameof(User), nameof(User.Name), name);
		}
	}
}