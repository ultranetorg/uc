using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			Environment.SetEnvironmentVariable(BootProductsPath.ToString(), productspath);
			Environment.SetEnvironmentVariable(BootSunAddress.ToString(),	sunaddress);
			Environment.SetEnvironmentVariable(BootSunApiKey.ToString(),	sunapikey);
			Environment.SetEnvironmentVariable(BootZone.ToString(),			zone.Name);
		}
	}
}
