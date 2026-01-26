using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class Simhash : IMetric<string>
{
	public const int Length = 64;

	public Simhash()
	{
	}

	public ulong Hashify(string a)
	{
		return Generate(a);
	}

	public int ComputeDistance(string a, string b)
	{
		return HammingDistance(Hashify(a), Hashify(b));
	}

	public int ComputeDistance(ulong a, ulong b)
	{
		return HammingDistance(a, b);
	}

	public static ulong Generate(string content, int width = 3)
	{
		return Generate(Tokenize(content, width));
	}

	public static ulong Generate(IEnumerable<string> tokens)
	{
		ulong[] v = new ulong[8];

		ulong mask = 0x0101_0101_0101_0101;

		int n = 0;

		foreach(var t in tokens)
		{
			throw new NotImplementedException();
			ulong hash = 0;// System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(t));

			for(int i = 0; i < 8; i++)
			{
				v[i] += hash & mask;

				hash >>= 1;
			}

			n++;
		}

		ulong h = 0;
		ulong m = (ulong)n/2;
		var b = 1UL;

		foreach(var i in v)
		{
			if(((i      ) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 8 ) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 16) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 24) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 32) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 40) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 48) & 0xff) > m)		h |= b;		b <<= 1;
			if(((i >> 56) & 0xff) > m)		h |= b;		b <<= 1;
		}

		return h;
	}
	
	public static ulong GenerateFromWord(string content)
	{
		return Generate(content.ToCharArray().Select(i => i.ToString()));
	}

	public static IEnumerable<string> Slide(string content, int width = 4)
	{
		for(int i = 0; i < content.Length + 1 - width; i++)
		{
			string piece = content.Substring(i, width);
			yield return piece;
		}
	}

	public static string Scrub(string content)
	{
		MatchCollection matches = Regex.Matches(content, @"[\w\u4e00-\u9fcc]+");
		string ans = "";

		foreach(Match match in matches)
		{
			ans += match.Value;
		}

		return ans;
	}

 	public static IEnumerable<string> Tokenize(string content, int width)
 	{
 		content = content.ToLower();
 		content = Scrub(content);
 		
 		return Slide(content, width);

		//return content.Split(' ');
 	}

    public static int HammingDistance(ulong hash1, ulong hash2)
    {
        return BitOperations.PopCount(hash1 ^ hash2);
    }
}
