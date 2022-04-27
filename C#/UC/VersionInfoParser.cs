using System;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace UC
{
	public class ParseInfo
	{
		internal ParseInfo(object key, object value)
		{
			this.key = key;
			this.value = value;
		}
		readonly object key;
		readonly object value;
		public object Key
		{
			get { return key; }
		}
		public object Value
		{
			get { return this.value; }
		}
	}

	/// <summary>
	/// push parser implementation to read VERSIONINFO resources 
	/// </summary>
	public sealed class VersionInfoParser
	{
		#region public types
		public enum KeyWords
		{
			VS_VERSION_INFO,
			VS_FIXEDFILEINFO,
			StringFileInfo,
			VarFileInfo,
			Translation,
			ResourceChecksum,
			FixedFileInfo,
			StringTable,
		}

		public enum StringKeys
		{
			Comments,
			CompanyName,
			FileDescription,
			FileVersion,
			InternalName,
			LegalCopyright,
			LegalTrademarks,
			OriginalFilename,
			PrivateBuild,
			ProductName,
			ProductVersion,
			SpecialBuild,

			FileFlagsMask,
			FileFlags,
			FileOS,
			FileType,
			FileSubtype,

			Language,
			CodePage,
			Checksum,
		}

		[Flags]
		public enum FileFlags
		{
			VS_FF_DEBUG = 0x00000001,
			VS_FF_PRERELEASE = 0x00000002,
			VS_FF_PATCHED = 0x00000004,
			VS_FF_PRIVATEBUILD = 0x00000008,
			VS_FF_INFOINFERRED = 0x00000010,
			VS_FF_SPECIALBUILD = 0x00000020,
			ALL = VS_FF_DEBUG | VS_FF_PRERELEASE | VS_FF_PATCHED | VS_FF_PRIVATEBUILD | VS_FF_INFOINFERRED | VS_FF_SPECIALBUILD,
		}

		public enum FileOS
		{
			VOS_UNKNOWN = 0x00000000,
			VOS_DOS = 0x00010000,
			VOS_OS216 = 0x00020000,
			VOS_OS232 = 0x00030000,
			VOS_NT = 0x00040000,
			VOS_WINCE = 0x00050000,

			VOS__BASE = 0x00000000,
			VOS__WINDOWS16 = 0x00000001,
			VOS__PM16 = 0x00000002,
			VOS__PM32 = 0x00000003,
			VOS__WINDOWS32 = 0x00000004,

			VOS_DOS_WINDOWS16 = 0x00010001,
			VOS_DOS_WINDOWS32 = 0x00010004,
			VOS_OS216_PM16 = 0x00020002,
			VOS_OS232_PM32 = 0x00030003,
			VOS_NT_WINDOWS32 = 0x00040004,
		}

		public enum FileType
		{
			VFT_UNKNOWN = 0x00000000,
			VFT_APP = 0x00000001,
			VFT_DLL = 0x00000002,
			VFT_DRV = 0x00000003,
			VFT_FONT = 0x00000004,
			VFT_VXD = 0x00000005,
			VFT_STATIC_LIB = 0x00000007,
		}

		public enum FileSubtype
		{
			//dwFileSubtype for VFT_WINDOWS_DRV
			VFT2_UNKNOWN = 0x00000000,
			VFT2_DRV_PRINTER = 0x00000001,
			VFT2_DRV_KEYBOARD = 0x00000002,
			VFT2_DRV_LANGUAGE = 0x00000003,
			VFT2_DRV_DISPLAY = 0x00000004,
			VFT2_DRV_MOUSE = 0x00000005,
			VFT2_DRV_NETWORK = 0x00000006,
			VFT2_DRV_SYSTEM = 0x00000007,
			VFT2_DRV_INSTALLABLE = 0x00000008,
			VFT2_DRV_SOUND = 0x00000009,
			VFT2_DRV_COMM = 0x0000000A,
			VFT2_DRV_INPUTMETHOD = 0x0000000B,
			VFT2_DRV_VERSIONED_PRINTER = 0x0000000C,

			//dwFileSubtype for VFT_WINDOWS_FONT
			VFT2_FONT_RASTER = 0x00000001,
			VFT2_FONT_VECTOR = 0x00000002,
			VFT2_FONT_TRUETYPE = 0x00000003,
		}
		#endregion

		#region public methods
		public static void Parse(string fileName, Action<ParseInfo> info)
		{
			byte[] data;
			int handle = 0;
			int size = Win32.GetFileVersionInfoSize(fileName, out handle);
			if (size == 0)
			{
				int error = Marshal.GetLastWin32Error();
				if (error == ERROR_RESOURCE_DATA_NOT_FOUND || error == ERROR_RESOURCE_TYPE_NOT_FOUND)
				{
					info(new ParseInfo("!", error));
					return;
				}
				throw new System.ComponentModel.Win32Exception(error, "GetFileVersionInfoSize");
			}
			data = new byte[size];
			if (!Win32.GetFileVersionInfo(fileName, handle, size, data))
			{
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(), "GetFileVersionInfo");
			}
			new VersionInfoParser(new Buffer(data), info).ParseVersionInfo();
		}
		#endregion

		#region private fields
		readonly StringKeys[] stringKeys = (StringKeys[])Enum.GetValues(typeof(StringKeys));
		readonly Action<ParseInfo> info;
		readonly Buffer buffer;
		const int ERROR_RESOURCE_DATA_NOT_FOUND = 1812;
		const int ERROR_RESOURCE_TYPE_NOT_FOUND = 1813;
		#endregion

		#region private functions

		private VersionInfoParser(Buffer buffer, Action<ParseInfo> info)
		{
			this.buffer = buffer;
			this.info = info;
		}

		private object ConvertToKeyEnum(string key)
		{
			int index = Array.FindIndex<StringKeys>(stringKeys, delegate(StringKeys match) { return match.ToString() == key; });
			return index >= 0 ? (object)stringKeys[index] : (object)key;
		}

		private void SwitchCodePage(int codePage)
		{
			try
			{
				Encoding newEncoding = Encoding.GetEncoding(codePage);
				if (buffer.Encoding != newEncoding)
				{
					switch ((buffer.Encoding.IsSingleByte ? 0 : 1) | (newEncoding.IsSingleByte ? 0 : 2))
					{
						case 0:
							// seems only to work when both are single byte
							buffer.Encoding = newEncoding;
							Info("*", newEncoding);
							break;
						case 1:
							this.Info("?", string.Format("wrong codepage ({0}) for 16bit resource ", codePage));
							break;
						case 2:
							this.Info("?", string.Format("wrong codepage ({0}) for 8bit resource", codePage));
							break;
						case 3:
							this.Info("?", string.Format("unusable codepage ({0}) '{1}'", codePage, newEncoding.EncodingName));
							break;
					}
				}
			}
			catch (NotSupportedException)
			{
				this.Info("?", string.Format("unsupported codepage {0}", codePage));
			}
		}

		private void Info(object key, object value)
		{
			this.info(new ParseInfo(key, value));
		}

		private void ParseVersionInfo()
		{
			UInt16 wLength = buffer.ReadUInt16();
			UInt16 wValueLength = buffer.ReadUInt16();
			if (buffer.GetUInt16At(0) == 0)
			{
				buffer.Encoding = System.Text.Encoding.Unicode;
				UInt16 type = buffer.ReadUInt16();
				if (type != 0)
				{
					this.Info("?", string.Format("type 1 expected, {0} found", type));
				}
			}
			else
			{
				buffer.Encoding = System.Text.Encoding.ASCII;
			}
			Info("*", buffer.Encoding);

			if (buffer.ReadString() != KeyWords.VS_VERSION_INFO.ToString())
			{
				throw new ApplicationException("VS_VERSION_INFO expected");
			}
			this.Info("{", KeyWords.VS_VERSION_INFO);
			buffer.Align();
			if (wValueLength == 52)
			{
				ParseFixedFileInfo();
			}
			else if (wValueLength != 0)
			{
				throw new ApplicationException("illegal wValueLength member");
			}
			while (buffer.Index < wLength)
			{
				buffer.Align();
				string peek = buffer.GetStringAt(buffer.Encoding.IsSingleByte ? 4 : 6);
				if (peek == KeyWords.StringFileInfo.ToString())
				{
					ParseStringFileInfo();
				}
				else if (peek == KeyWords.VarFileInfo.ToString())
				{
					ParseVarFileInfo();
				}
				else if (peek == KeyWords.Translation.ToString())
				{
					ParseVar();
				}
				else
				{
					throw new ApplicationException("illegal szKey member, only StringFileInfo and VarFileInfo allowed");
				}
			}
			this.Info("}", KeyWords.VS_VERSION_INFO);
		}

		private void ParseVarFileInfo()
		{
			buffer.Align();
			int endIndex = buffer.Index + buffer.ReadUInt16();
			if (buffer.ReadUInt16() != 0)
			{
				throw new ApplicationException("wValueLength must be zero");
			}
			if (!buffer.Encoding.IsSingleByte)
			{
				UInt16 type = buffer.ReadUInt16();
				if (type == 0)
				{
					this.Info("?", "type 1 (text) expected, 0 (binary) found");
				}
				else if (type != 1)
				{
					throw new ApplicationException("illegal type, only type=1 (text) and type=0 (binary) are supported");
				}
			}
			if (buffer.ReadString() != KeyWords.VarFileInfo.ToString())
			{
				throw new ApplicationException("VarFileInfo expected");
			}
			this.Info("{", KeyWords.VarFileInfo);

			while (buffer.Index < endIndex)
			{
				ParseVar();
			}
			this.Info("}", KeyWords.VarFileInfo);
		}

		private void ParseVar()
		{
			buffer.Align();
			int endIndex = buffer.Index + buffer.ReadUInt16();
			UInt16 wValueLength = buffer.ReadUInt16();
			if (!buffer.Encoding.IsSingleByte)
			{
				if (buffer.ReadUInt16() != 0)
				{
					throw new ApplicationException("illegal type, only type=0 (binary) is supported");
				}
			}

			string str = buffer.ReadString();
			buffer.Align();
			if (str == KeyWords.Translation.ToString())
			{
				this.Info("{", str);
				while (wValueLength > 0)
				{
					this.Info(StringKeys.Language, buffer.ReadUInt16());
					this.Info(StringKeys.CodePage, buffer.ReadUInt16());
					wValueLength -= 4;
				}
				this.Info("}", str);
			}
			else if (str == KeyWords.ResourceChecksum.ToString())
			{
				this.Info(StringKeys.Checksum, buffer.ReadBytes(wValueLength));
			}
			else
			{
				throw new ApplicationException("Translation or ResourceChecksum expected");
			}

			if (endIndex != buffer.Index)
			{
				throw new ApplicationException("wrong size in String structure");
			}
		}

		private void ParseStringFileInfo()
		{
			buffer.Align();
			int endIndex = buffer.Index + buffer.ReadUInt16();
			if (buffer.ReadUInt16() != 0)
			{
				throw new ApplicationException("wValueLength must be zero");
			}
			if (!buffer.Encoding.IsSingleByte)
			{
				UInt16 type = buffer.ReadUInt16();
				if (type == 0)
				{
					this.Info("?", "type 1 (text) expected, 0 (binary) found");
				}
				else if (type != 1)
				{
					throw new ApplicationException("illegal type, only type=1 (text) and type=0 (binary) are supported");
				}
			}
			if (buffer.ReadString() != KeyWords.StringFileInfo.ToString())
			{
				throw new ApplicationException("StringFileInfo expected");
			}
			this.Info("{", KeyWords.StringFileInfo);
			while (buffer.Index < endIndex)
			{
				ParseStringTable();
			}
			this.Info("}", KeyWords.StringFileInfo);
		}

		private void ParseStringTable()
		{
			buffer.Align();
			int endIndex = buffer.Index + buffer.ReadUInt16();
			if (buffer.ReadUInt16() != 0)
			{
				throw new ApplicationException("wValueLength must be zero");
			}
			if (!buffer.Encoding.IsSingleByte)
			{
				UInt16 type = buffer.ReadUInt16();
				if (type == 0)
				{
					this.Info("?", "type 1 (text) expected, 0 (binary) found");
				}
				else if (type != 1)
				{
					throw new ApplicationException("illegal type, only type=1 (text) and type=0 (binary) are supported");
				}
			}
			string szKey = buffer.ReadString();
			if (szKey.Length != 8)
			{
				throw new ApplicationException("8-digit hexadecimal number expected");
			}

			int language;
			if (!int.TryParse(szKey.Substring(0, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out language))
			{
				throw new ApplicationException("hexadecimal number expected, language");
			}

			int codePage;
			if (!int.TryParse(szKey.Substring(4, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out codePage))
			{
				throw new ApplicationException("hexadecimal number expected, code page");
			}

			SwitchCodePage(codePage);

			this.Info("{", KeyWords.StringTable);
			this.Info(StringKeys.Language, language);
			this.Info(StringKeys.CodePage, codePage);

			while (buffer.Index < endIndex)
			{
				ParseString();
			}
			this.Info("}", KeyWords.StringTable);
		}

		private void ParseString()
		{
			buffer.Align();
			int endIndex = buffer.Index + buffer.ReadUInt16();
			UInt16 wValueLength = buffer.ReadUInt16();
			UInt16 type = 1;
			if (!buffer.Encoding.IsSingleByte)
			{
				type = buffer.ReadUInt16();
				if (type != 1 && type != 0)
				{
					throw new ApplicationException("illegal type, only type=1 (text) and type=0 (binary) are supported");
				}
			}
			string szKey = buffer.ReadString();
			string value = "";
			buffer.Align();
			if (wValueLength != 0)
			{
				value = type == 1 ? buffer.ReadString() : buffer.ReadBytes(wValueLength * 2);
			}

			if (endIndex != buffer.Index)
			{
				buffer.Index = endIndex;
			}
			this.Info(ConvertToKeyEnum(szKey), value);
		}

		private void ParseFixedFileInfo()
		{
			this.Info("{", KeyWords.VS_FIXEDFILEINFO);
			if (buffer.ReadUInt32() != 0xfeef04bd)
			{
				throw new ApplicationException("illegal signature");
			}
			UInt32 dwStrucVersion = buffer.ReadUInt32();
			if (dwStrucVersion != 0x00010000)
			{
				throw new ApplicationException("illegal structure version");
			}
			this.Info(StringKeys.FileVersion, buffer.ReadVersion());
			this.Info(StringKeys.ProductVersion, buffer.ReadVersion());
			this.Info(StringKeys.FileFlagsMask, (FileFlags)buffer.ReadUInt32());
			this.Info(StringKeys.FileFlags, (FileFlags)buffer.ReadUInt32());
			this.Info(StringKeys.FileOS, (FileOS)buffer.ReadUInt32());
			this.Info(StringKeys.FileType, (FileType)buffer.ReadUInt32());
			this.Info(StringKeys.FileSubtype, (FileSubtype)buffer.ReadUInt32());
			buffer.ReadUInt32(); // dwFileDateMS
			buffer.ReadUInt32(); // dwFileDateLS
			this.Info("}", KeyWords.VS_FIXEDFILEINFO);
		}
		#endregion

		#region private types
		private class Buffer
		{
			#region private fields
			Encoding enc = System.Text.Encoding.Unicode;
			private byte[] buffer;
			private int index;
			#endregion

			#region internal functions
			internal Encoding Encoding
			{
				get { return enc; }
				set { enc = value; }
			}

			internal int Index
			{
				get { return index; }
				set { index = value; }
			}

			internal Buffer(byte[] buffer)
			{
				this.buffer = buffer;
				this.index = 0;
			}
			#endregion

			#region buffer access functions
			internal void Align()
			{
				index = ((index + 3) / 4) * 4;
			}

			internal UInt16 ReadUInt16()
			{
				index += 2;
				return BitConverter.ToUInt16(buffer, index - 2);
			}

			internal UInt16 GetUInt16At(int offset)
			{
				return BitConverter.ToUInt16(buffer, index + offset);
			}

			internal UInt32 ReadUInt32()
			{
				index += 4;
				return BitConverter.ToUInt32(buffer, index - 4);
			}

			internal Version ReadVersion()
			{
				ushort minor = ReadUInt16();
				ushort major = ReadUInt16();
				ushort revision = ReadUInt16();
				ushort build = ReadUInt16();
				return new Version(major, minor, build, revision);
			}

			internal string ReadString()
			{
				int length = 0;
				if (enc.IsSingleByte)
				{
					while (buffer[index + length] != 0)
					{
						length += 1;
					}
					string str = enc.GetString(buffer, index, length);
					index = index + length + 1;
					return str;
				}
				else
				{
					while ((buffer[index + length] + buffer[index + length + 1]) != 0)
					{
						length += 2;
					}
					string str = enc.GetString(buffer, index, length);
					index = index + length + 2;
					return str;
				}
			}

			internal string GetStringAt(int offset)
			{
				int length = 0;
				if (enc.IsSingleByte)
				{
					while (buffer[index + offset + length] != 0)
					{
						length += 1;
					}
				}
				else
				{
					while ((buffer[index + offset + length] + buffer[index + offset + length + 1]) != 0)
					{
						length += 2;
					}
				}
				return enc.GetString(buffer, index + offset, length);
			}

			internal string ReadBytes(int length)
			{
				string value = BitConverter.ToString(buffer, index, length);
				index += length;
				return value;
			}
			#endregion
		}

		private static class Win32
		{
			[DllImport("version.dll", SetLastError = true)]
			public static extern int GetFileVersionInfoSize(string fileName, out int handle);

			[DllImport("version.dll", SetLastError = true)]
			public static extern bool GetFileVersionInfo(string fileName, int handle, int length, byte[] data);
		}
		#endregion
	}
}
