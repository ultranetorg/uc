using System.Runtime.CompilerServices;

namespace Uccs.Fair;

public class AccountAddressValidator
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Validate(string accountAddress)
	{
		if(!Regexes.AccountAddress.IsMatch(accountAddress))
		{
			throw new InvalidAccountAddressException(accountAddress);
		}
	}
}
