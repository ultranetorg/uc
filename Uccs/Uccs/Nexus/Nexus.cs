using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Uccs.Net;

namespace Uccs.Uos
{
	public class Nexus
	{
		public const string BootProductsPath	= "Uos.Nexus.ProductsPath";
		public const string BootSunAddress 		= "Uos.Nexus.SunAddress"; 
		public const string BootSunApiKey		= "Uos.Nexus.SunApiKey"; 
		public const string BootZone		 	= "Uos.Nexus.Zone"; 

		public Nexus(string productspath, string sunaddress, string sunapikey, Zone zone)
		{
			Environment.SetEnvironmentVariable(BootProductsPath,productspath);
			Environment.SetEnvironmentVariable(BootSunAddress,	sunaddress);
			Environment.SetEnvironmentVariable(BootSunApiKey,	sunapikey);
			Environment.SetEnvironmentVariable(BootZone,		zone.Name);
		}

    }
}
