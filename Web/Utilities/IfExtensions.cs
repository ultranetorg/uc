using System.Text.RegularExpressions;

namespace Uccs.Web.Utilities;

public static class BoolExtensions
{
	public static Condition False(this Result<bool> result)
	{
		return new Condition(result.Value == false);
	}

	public static Condition True(this Result<bool> result)
	{
		return new Condition(result.Value == true);
	}
}

public static class IntExtensions
{
	public static Condition IsNegative(this Result<int> result)
	{
		return new Condition(result.Value < 0);
	}
}

public static class LongExtensions
{
	public static Condition IsNegative(this Result<long> result)
	{
		return new Condition(result.Value < 0);
	}
}

public static class StringExtensions
{
	public static Condition IsNullOrEmpty(this Result<string?> result)
	{
		return new Condition(string.IsNullOrEmpty(result.Value));
	}

	public static Condition NotMatch(this Result<string> result, Regex regex)
	{
		bool isMatch = false;
		try
		{
			isMatch = regex.IsMatch(result.Value);
		}
		catch (RegexMatchTimeoutException)
		{
		}

		return new Condition(!isMatch);
	}

	public static Condition LengthNotEquals(this Result<string> result, int length)
	{
		return new Condition(result.Value.Length != length);
	}

	public static Condition LessThan(this Result<string> result, int length)
	{
		return new Condition(result.Value.Length < length);
	}

	public static Condition GreaterThan(this Result<string> result, int length)
	{
		return new Condition(result.Value.Length > length);
	}
}

public static class ObjectExtensions
{
	public static Condition IsNull(this Result<object?> result)
	{
		return new Condition(result.Value == null);
	}

	public static Condition IsNotNull(this Result<object?> result)
	{
		return new Condition(result.Value != null);
	}
}
