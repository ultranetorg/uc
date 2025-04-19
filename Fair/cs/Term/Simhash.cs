using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class Simhash
{
	public const int Length = 64;

	public Simhash()
	{
	}

	public static ulong Generate(string content)
	{
		var tokens = Tokenize(content);
		int[] v = new int[Length];

		foreach(var t in tokens)
		{
			ulong hash = System.IO.Hashing.XxHash64.HashToUInt64(Encoding.UTF8.GetBytes(t));

			for(int i = 0; i < Length; i++)
			{
				if(((hash >> i) & 1) == 1)
					v[i] += 1;
				else
					v[i] -= 1;
			}
		}

		ulong h = 0;

		for(int i = 0; i < Length; i++)
		{
			if(v[i] > 0)
				h |= (1UL << i);
		}

		return h;
	}

	public static ulong GenerateFast(string content)
	{
		var tokens = Tokenize(content);
		ulong[] v = new ulong[8];

		ulong mask = 0x0101_0101_0101_0101;

		foreach(var t in tokens)
		{
			ulong hash = System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(t));

			for(int i = 0; i < 8; i++)
			{
				v[i] += hash & mask;

				hash >>= 1;
			}
		}

		ulong h = 0;
		ulong m = (ulong)tokens.Length/2;
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
		int[] v = new int[Length];

		foreach(var t in content.ToLowerInvariant())
		{
			ulong hash = System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(t.ToString()));
			//ulong hash = BitConverter.ToUInt64(_jenkinsOneAtATime.ComputeHash(Encoding.UTF8.GetBytes(t.ToString())).Hash, 0);

			for(int i = 0; i < Length; i++)
			{
				if(((hash >> i) & 1) == 1)
					v[i] += 1;
				else
					v[i] -= 1;
			}
		}

		ulong h = 0;

		for(int i = 0; i < Length; i++)
		{
			if(v[i] > 0)
				h |= (1UL << i);
		}

		return h;
	}

	public static string[] Slide(string content, int width = 4)
	{
		var listOfShingles = new string[content.Length + 1 - width];

		for(int i = 0; i < listOfShingles.Length; i++)
		{
			string piece = content.Substring(i, width);
			listOfShingles[i] = piece;
		}

		return listOfShingles;
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

 	public static string[] Tokenize(string content, int width = 3)
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
