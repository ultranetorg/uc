using System;
using System.Collections.Generic;
using System.Text;

namespace Uccs
{
	public class OS : StringEnum<OS>
	{
		public const string Windows	= "Windows";
		public const string Linux	= "Linux";
		public const string MacOS	= "macOS";
		public const string Android	= "Android";
		public const string iOS		= "iOS";
		public const string Uos		= "Uos";
	}

	public class Architecture : StringEnum<OS>
	{
		public const string X86		= "x86";
		public const string X64		= "x64";
		public const string IA64	= "ia64";
		public const string Arm		= "arm";
		public const string Arm64	= "arm64";
		public const string CLR		= "clr";
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
