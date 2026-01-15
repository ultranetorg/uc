namespace Uccs;

public class BytesComparer : IComparer<byte[]>
{
	public int Compare(byte[] x, byte[] y)
	{
		var len = Math.Min(x.Length, y.Length);

		for(var i = 0; i < len; i++)
		{
			var c = x[i].CompareTo(y[i]);

			if(c != 0)
			{
				return c;
			}
		}

		return x.Length.CompareTo(y.Length);
	}
}

public class BytesEqualityComparer : IEqualityComparer<byte[]>
{
	public bool Equals(byte[] a, byte[] b)
	{
		if(a.Length != b.Length)
			return false;

		for(int i = 0; i < a.Length; i++)
			if(a[i] != b[i])
				return false;

		return true;
	}

	public int GetHashCode(byte[] b)
	{
		return b[0].GetHashCode();
	}
}

public static class Bytes
{
	public static BytesEqualityComparer EqualityComparer = new BytesEqualityComparer();
	public static BytesComparer			Comparer = new BytesComparer();

	public static byte[] Xor(byte[] a, byte[] b)
	{
		if(a.Length == b.Length)
		{
			var r = new byte[a.Length];

			for(var i = 0; i < a.Length; i++)
				r[i] = (byte)(a[i] ^ b[i]);

			return r;
		}
		else
		{
			var n = Math.Min(a.Length, b.Length);
			var max = a.Length > b.Length ? a : b;
			var r = new byte[max.Length];

			Array.Copy(max, r, r.Length);

			for(var i = 0; i < n; i++)
				r[i] = (byte)(a[i] ^ b[i]);

			return r;
		}
	}

	public static byte[] Xor(Span<byte> a, Span<byte> b)
	{
		if(a.Length == b.Length)
		{
			var r = new byte[a.Length];

			for(var i = 0; i < a.Length; i++)
				r[i] = (byte)(a[i] ^ b[i]);

			return r;
		}
		else
		{
			var n = Math.Min(a.Length, b.Length);
			var max = a.Length > b.Length ? a : b;
			var r = new byte[max.Length];

			max.CopyTo(r);
			//Array.Copy(, r, r.Length);

			for(var i = 0; i < n; i++)
				r[i] = (byte)(a[i] ^ b[i]);

			return r;
		}
	}
}
