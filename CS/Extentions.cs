using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Text;

namespace Uccs;

public static class Extentions
{
	public static bool IsSet(this long x, long i)
	{
		return (x & i) != 0;
	}

	public static string ToHex(this byte[] e)
	{
		return Convert.ToHexString(e);
	}

	public static string ToHex(this byte[] e, int max)
	{
		return e.Length <= max ? Convert.ToHexString(e) : (Convert.ToHexString(e, 0, max/2 - 1 + max%2) + "...." + Convert.ToHexString(e, e.Length - max/2 + 1, max/2 - 1));
	}

	public static byte[] FromHex(this string e)
	{
		return Convert.FromHexString(e);
	}

	public static byte[] FromHex(this ReadOnlySpan<char> e)
	{
		return Convert.FromHexString(e);
	}

	public static string ToHexPrefix(this byte[] e)
	{
		return Convert.ToHexString(e, 0, 4);
	}

	public static T Random<T>(this IEnumerable<T> e)
	{
		return e.OrderByRandom().First();
	}

	public static T RandomOrDefault<T>(this IEnumerable<T> e)
	{
		return e.Any() ? e.OrderByRandom().First() : default;
	}

	public static IEnumerable<T> OrderByRandom<T>(this IEnumerable<T> e)
	{
		return e.OrderBy(i => Guid.NewGuid());
	}

	public static OrderedDictionary<K, V> ToOrderedDictionary<T, K, V>(this IEnumerable<T> e, Func<T, K> k, Func<T, V> v)
	{
		var c = new OrderedDictionary<K, V>();

		foreach(var i in e)
		{
			c.Add(k(i), v(i));
		}

		return c;
	}

	public static bool Contains(this Exception e, Func<Exception, bool> p)
	{
		if(p(e))
			return true;

		if(e.InnerException != null && e.InnerException.Contains(p))
			return true;

		if(e is AggregateException ae)
			foreach(var i in ae.InnerExceptions)
				if(i.Contains(p))
					return true;

		return false;
	}

	public static string ToFlagsString(this Enum value)
    {
        long v = Convert.ToInt64(value);

        if(v == 0)
        {
            return string.Empty;
        }

        return value.ToString();
    }
}
