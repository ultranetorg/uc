using System.Text.RegularExpressions;

namespace Uccs.Fair;

public static class Regexes
{
	private const string EntityIdPattern = @"^\d+-\d$";

	public static Regex EntityId = new Regex(EntityIdPattern, RegexOptions.Compiled);
}
