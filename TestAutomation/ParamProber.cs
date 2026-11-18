using System;
using System.Collections.Generic;

namespace TestAutomation.ParamProber;

public enum ProbeType : uint
{
	None,
	Latin, // Latin alphabet string without special char-s, spaces or numbers.
	LatinSpecial, // Latin alphabet string with special characters but without spaces.
	LatinSpaced, // Latin alphabet string with spaces.
	Numbers,
	NumberDecimal,
	NumbersSpaced,
	NumbersSpecial,
	NumbersBig,

	// Ultranet specific:
	ShortAccountAddress,
	LongAccountAddress
}

class Prober
{

	// TODO: Empty input.
	// TODO: Singe digit number.
	// TODO: Negative number.
	// TODO: Huge input. Huge like 2MB of string.
	static Dictionary<ProbeType, string> probes = new(){
		{ProbeType.Latin, "Lorem_ipsum_dolor"}, // Underscore considered not a special char.
		{ProbeType.LatinSpecial, "Lo*rem@ipsum#dolor$"},
		{ProbeType.LatinSpaced, "Lorem ipsum dolor"},
		{ProbeType.Numbers, "152870"},
		{ProbeType.NumbersBig, "152870001234170923010517302910192375092301935720"},
		{ProbeType.NumberDecimal, "15287.0234503"},
		{ProbeType.NumbersSpaced, "15287 02345 03495"},
		{ProbeType.NumbersSpecial, "15*287@02345#03495$"},

		// Ultranet specific:
		{ProbeType.ShortAccountAddress, "0x8512034" },
		{ProbeType.LongAccountAddress, "0xA8DEC1FFBD0E1D5FAA22597AD59B01D56438359A597AD59B" }
	};

	///<summary> Substitute the {0} indicator in the string with some 'ProbeType' bound test string
	/// and invoke some action using the created string. 
	///</summary>
	///<param name="only"> Probe types to run. Default 'null' to run all. </param>
	///<param name="exclude"> Probe types to not run. Default 'null' to exclude none. </param>
	public static void Probe(string formatStr, Action<string, ProbeType> defaultAction, Dictionary<ProbeType, Action<string, ProbeType>> specificActions = null,
			List<ProbeType> only = null, List<ProbeType> exclude = null)
	{
		foreach (ProbeType pt in Enum.GetValues(typeof(ProbeType))) {
			if (pt == ProbeType.None) continue;

			// Only run probes in `only` if any.
			if (only != null && !only.Contains(pt)) continue;

			// Exclude probes mentioned in `probesExclude`.
			if (exclude != null && exclude.Contains(pt)) continue;

			if (!formatStr.Contains("{0}")) throw new Exception("Format string should include \"{0}\" substring.");
			string s = string.Format(formatStr, probes[pt]);
			if (specificActions != null && specificActions.ContainsKey(pt)) specificActions[pt](s, pt);
			else defaultAction(s, pt);
		}
	}
}
