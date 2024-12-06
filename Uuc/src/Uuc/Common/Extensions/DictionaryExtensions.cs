namespace Uuc.Common.Extensions;

public static class DictionaryExtensions
{
	public static bool TryGetString(this IDictionary<string, object> source, string key, out string? value)
	{
		bool result = source.TryGetValue(key, out object? dictionaryValue);
		value = result ? Convert.ToString(dictionaryValue) : null;
		return result;

	}
}
