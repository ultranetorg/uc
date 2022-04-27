using System;
using System.Collections.Generic;
using System.Linq;

namespace UC.Net
{
    public class BytesComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
			var len = Math.Min(x.Length, y.Length);

			for (var i = 0; i < len; i++)
			{
			    var c = x[i].CompareTo(y[i]);
			    if (c != 0)
			    {
			        return c;
			    }
			}

			return x.Length.CompareTo(y.Length);
        }
    }

	class BytesEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] b1, byte[] b2)
        {
            return b1.SequenceEqual(b2);
        }

        public int GetHashCode(byte[] b)
        {
            return b[0].GetHashCode();
        }
    }

    public static class Bytes
	{
        public static byte[] Xor(byte[] a, byte[] b)
        {
            var n = Math.Min(a.Length, b.Length);
			var max = a.Length > b.Length ? a : b;
            var result = new byte[max.Length];

			Array.Copy(max, result, result.Length);

            for(var i = 0; i < n; i++)
                result[i] = (byte) (a[i] ^ b[i]);

            return result;
        }
	}
}
