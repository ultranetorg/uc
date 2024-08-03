using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Uccs
{
	public class Platform// : IBinarySerializable
	{
		public PlatformFamily	Family;
		public int				Brand;
		public int				Version;
		public Architecture		Architecture;

		public static Platform	Current { get; protected set; }

		static Platform()
		{
			Current = new Platform();

			if(RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))	Current.Family = PlatformFamily.Unix; else
			if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))	Current.Family = PlatformFamily.Unix; else
			if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))		Current.Family = PlatformFamily.macOS; else
			if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Current.Family = PlatformFamily.Windows;
				Current.Brand = (int)WindowsBrand.MicrosoftWindows;
				
				MicrosoftWindowsVersion v = MicrosoftWindowsVersion.Unknown;
				Enum.TryParse<MicrosoftWindowsVersion>("NT_" + RuntimeInformation.OSDescription.Split(' ').Last().Replace('.', '_'), out v);
				Current.Version = (int)v;
			}

			Current.Architecture =	RuntimeInformation.OSArchitecture switch
									{
										System.Runtime.InteropServices.Architecture.X86			=> Architecture.x86_32 ,
										System.Runtime.InteropServices.Architecture.X64			=> Architecture.x86_64 ,
										System.Runtime.InteropServices.Architecture.Arm			=> Architecture.ARM,
										System.Runtime.InteropServices.Architecture.Armv6		=> Architecture.ARMv6,
										System.Runtime.InteropServices.Architecture.Arm64		=> Architecture.ARM64,
										System.Runtime.InteropServices.Architecture.S390x		=> Architecture.S390x,
										System.Runtime.InteropServices.Architecture.LoongArch64 => Architecture.LoongArch64,
										System.Runtime.InteropServices.Architecture.Ppc64le		=> Architecture.PowerPC64LE,
										System.Runtime.InteropServices.Architecture.Wasm		=> Architecture.Wasm,
										_ => Architecture.Unknown
									};
		}

		public static object ParseIdentifier(string t)
		{
			var p = t.Split('/');

			var f = Enum.Parse<PlatformFamily>(p[0]);
			
			if(p.Skip(1).Any())
			{
				var b = Enum.Parse(typeof(Platform).Assembly.GetType(typeof(Platform).Namespace + '.' + f + "Brand"), p[1]);
				
				if(p.Skip(2).Any())
				{
					return Convert.ToInt32(Enum.Parse(typeof(Platform).Assembly.GetType(typeof(Platform).Namespace + '.' + b + "Version"), p[2]));
				}

				return Convert.ToInt32(b);
			}
			
			return f;
		}

		public void Read(BinaryReader reader)
		{
			Family			= (PlatformFamily)reader.ReadByte();
			Brand			= reader.Read7BitEncodedInt();
			Version			= reader.Read7BitEncodedInt();
			Architecture	= (Architecture)reader.ReadByte();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)Family);
			writer.Write7BitEncodedInt((int)Brand);
			writer.Write7BitEncodedInt((int)Version);
			writer.Write((byte)Architecture);
		}
	}

	public enum PlatformFamily : byte
	{
		Unknown = 0,

		Android	= 40,
		iOS		= 80,
		macOS	= 120,
		Unix	= 160,
		UOS		= 200,
		Windows	= 240,
	}

	public enum PlatformBrand : byte
	{
		Unknown = 0,
	}

	public enum PlatformVersion : byte
	{
		Unknown = 0,
	}

	public enum Architecture : byte
	{
		Unknown = 0,

		x86_32 = 1,
		x86_64 = 2,
		IA64 = 3,
		ARM = 4,
		ARMv6 = 5,
		ARM64 = 6,
		S390x  = 7,
		LoongArch64 = 8,
		PowerPC64LE	= 9,

		Wasm = 100,
	}

	public enum WindowsBrand : byte
	{
		MicrosoftWindows = 10,
		MicrosoftWindowsServer = 20
	}

	public enum MicrosoftWindowsVersion : byte
	{
		Unknown = 0,
		_1_01 = 10,
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
		NT_10_0_10240,
		NT_10_0_10586,
		NT_10_0_14393,
		NT_10_0_15063,
		NT_10_0_16299,
		NT_10_0_17134,
		NT_10_0_17763,
		NT_10_0_18362,
		NT_10_0_18363,
		NT_10_0_19041,
		NT_10_0_19042,
		NT_10_0_19043,
		NT_10_0_19044,
		NT_10_0_19045,
		NT_10_0_22000,
		NT_10_0_22621,
		NT_10_0_22631,
	}

	public enum MicrosoftWindowsServer : byte
	{
		NT_3_1 = 10,
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
