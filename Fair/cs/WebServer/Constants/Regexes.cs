using System.Text.RegularExpressions;

namespace Uccs.Fair;

public static class Regexes
{
	private const string AccountAddressPattern = @"^0x[a-fA-F0-9]{40}$";
	public static Regex AccountAddress = new Regex(AccountAddressPattern, RegexOptions.Compiled);

	private const string EntityIdPattern = @"^\d+-\d$";
	public static Regex EntityId = new Regex(EntityIdPattern, RegexOptions.Compiled);

	private const string UserNamePattern = @"^[a-z0-9]+$";
	public static Regex UserName = new Regex(UserNamePattern, RegexOptions.Compiled);
}