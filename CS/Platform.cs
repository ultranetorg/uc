namespace Uccs
{
	public enum OperatingSystemFamily
	{
		Unix	= 1,
		Windows	= 2,
		MacOS	= 3,
		Android	= 4,
		iOS		= 5,
		Uos		= 6,
	}

	public enum Architecture
	{
		X86	= 1,
		X64 = 2,
		IA64 = 3,
		ARM = 4,
		ARM64 = 5,
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
