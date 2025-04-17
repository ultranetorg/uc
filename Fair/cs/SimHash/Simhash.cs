using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Uccs.Fair;

public class Simhash
{
	public int fpSize = 64;

	public ulong value { get; set; }

	public Simhash()
	{
	}

	public Simhash(Simhash simHash)
	{
		value = simHash.value;
	}

	public Simhash(ulong fingerPrint)
	{
		value = fingerPrint;
	}

	public void GenerateSimhash(string content)
	{
		var shingles = Shingling.tokenize(content);

		int[] v = new int[fpSize];
		ulong[] masks = setupMasks();

		foreach(string feature in shingles)
		{
			///ulong h = hashfuncjenkins(feature);
			ulong h = System.IO.Hashing.XxHash3.HashToUInt64(Encoding.UTF8.GetBytes(feature));

			int w = 1;

			for(int i = 0; i < fpSize; i++)
			{
				ulong result = h & masks[i];
				v[i] += (result > 0) ? w : -w;
			}
		}

		value = makeFingerprint(v, masks);
	}

	private ulong makeFingerprint(int[] v, ulong[] masks)
	{
		ulong ans = 0;
		for(int i = 0; i < fpSize; i++)
		{
			if(v[i] >= 0)
			{
				ans |= masks[i];
			}
		}
		return ans;
	}

	private ulong[] setupMasks()
	{
		ulong[] masks = new ulong[fpSize];
		for(int i = 0; i < masks.Length; i++)
		{
			masks[i] = (ulong)1 << i;
		}
		return masks;
	}
}

public class Shingling
{
	public static List<string> slide(string content, int width = 4)
	{
		var listOfShingles = new List<string>();
		for(int i = 0; i < (content.Length + 1 - width); i++)
		{
			string piece = content.Substring(i, width);
			listOfShingles.Add(piece);
		}
		return listOfShingles;
	}

	public static string scrub(string content)
	{
		MatchCollection matches = Regex.Matches(content, @"[\w\u4e00-\u9fcc]+");
		string ans = "";
		foreach(Match match in matches)
		{
			ans += match.Value;
		}

		return ans;
	}

	public static List<string> tokenize(string content, int width = 4)
	{
		content = content.ToLower();
		content = scrub(content);
		return slide(content, width);
	}
}
