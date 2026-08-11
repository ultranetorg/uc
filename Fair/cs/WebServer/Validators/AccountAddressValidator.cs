using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public static class AccountAddressValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Validate(string accountAddress)
	{
		if(!Regexes.AccountAddress.IsMatch(accountAddress))
		{
			throw new InvalidAccountAddressException(accountAddress);
		}
	}
}
