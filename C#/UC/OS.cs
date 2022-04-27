using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace UC
{
	public class OS
	{
		public static Platform Platform = Platform.Unknown;

		static OS()
		{
			if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Platform = Platform.Windows; else
			if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))	Platform = Platform.Linux; else
			if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))		Platform = Platform.MacOS;
		}
	}
}
