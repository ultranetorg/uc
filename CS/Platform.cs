namespace Uccs
{
	public class Platform
	{
		public PlatformFamily	Family;
		public byte				Brand;
		public byte				Version;
		public byte				Architecture;
	}

	public enum PlatformFamily : byte
	{
		Any = 0,

		Android	= 40,
		iOS		= 80,
		macOS	= 120,
		Unix	= 160,
		UOS		= 200,
		Windows	= 240,

		Last = Windows,
	}

	public enum WindowsBrand : byte
	{
		Any = 0,

		MicrosoftWindows = 1,
		MicrosoftWindowsServer = 1,

		Last = MicrosoftWindowsServer,
	}

	public enum MicrosoftWindows : byte
	{
		Any = 0,
		_1_01,
		_1_02,
		_1_03,
		_1_04,
		_2_01,
		_2_03,
		_2_10,
		_2_11,
		_3_00,
		_3_10,
		NT_3_1,
		_3_11,
		_3_2,
		NT_3_5,
		NT_3_51,
		_4_00,
		NT_4_0,
		_4_10,
		NT_5_0,
		_4_90,
		NT_5_1_2600,
		NT_5_1_2700,
		NT_5_1_2710,
		NT_5_2,
		NT_6_0,
		NT_6_1,
		NT_6_2,
		NT_6_3,
		NT_10_10240,
		NT_10_10586,
		NT_10_14393,
		NT_10_15063,
		NT_10_16299,
		NT_10_17134,
		NT_10_17763,
		NT_10_18362,
		NT_10_18363,
		NT_10_19041,
		NT_10_19042,
		NT_10_19043,
		NT_10_19044,
		NT_10_19045,
		NT_10_22000,
		NT_10_22621,
		NT_10_22631,
		Last = NT_10_22631
	}

	public enum MicrosoftWindowsServer : byte
	{
		Any = 0,

		NT_3_1,
		NT_3_5,
		NT_3_51,
		NT_4_0,
		NT_5_0,
		NT_5_2,
		NT_6_0,
		NT_6_1,
		NT_6_2,
		NT_6_3,
		NT_10_0_14393,
		NT_10_0_16299,
		NT_10_0_17134,
		NT_10_0_17763,
		NT_10_0_18362,
		NT_10_0_18363,
		NT_10_0_19041,
		NT_10_0_19042,
		NT_10_0_20348,
		NT_10_0_25398,

		Last = NT_10_0_25398
	}

	public enum Architecture : byte
	{
		Any = 0,

		x86_32 = 1,
		x86_64 = 2,
		IA64 = 3,
		ARM = 4,
		ARM64 = 5,

		Last = ARM64
	}

// 	public class Platform : IEquatable<Platform>
// 	{
// 		public string	System;
// 		public string	SystemTitle => OS.NameByValue(System);
// 		//public string	Modification;
// 		public string	Architecture;
// 		//public Version	Version;
// 
//  		public const char Separator = '.';
// 
// 
// 		public Platform(string family, string architecture)
// 		{
// 			System = family;
// 			//Modification = modification;
// 			Architecture = architecture;
// 			//Version = version;
// 		}
// 
// 		public static Platform Parse(string val)
// 		{
// 			var c = val.Split(Separator);
// 
// 			return new Platform(c[0], c[1]);
// 		}
// 
// 		public override string ToString()
// 		{
// 			return $"{System}{Separator}{Architecture}";
// 		}
// 
// 		public override bool Equals(object obj)
// 		{
// 			return Equals(obj as Platform);
// 		}
// 
// 		public bool Equals(Platform other)
// 		{
// 			return other != null &&
// 				   System == other.System &&
// 				  // Modification == other.Modification &&
// 				   Architecture == other.Architecture
// 				   //EqualityComparer<Version>.Default.Equals(Version, other.Version)
// 				   ;
// 		}
// 
// 		public override int GetHashCode()
// 		{
// 			return HashCode.Combine(System, Architecture);
// 		}
// 
// 		public static bool operator== (Platform left, Platform right)
// 		{
// 			return EqualityComparer<Platform>.Default.Equals(left, right);
// 		}
// 
// 		public static bool operator!= (Platform left, Platform right)
// 		{
// 			return !(left == right);
// 		}
// 	}
}
