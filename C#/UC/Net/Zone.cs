using System;

namespace UC.Net
{
	public class Zone : StringEnum<Zone>
	{
		public const string Localnet	= "Localnet";
		public const string Testnet0	= TestnetPrefix + "0";
		public const string Testnet1	= TestnetPrefix + "1";
		public const string Mainnet		= "Mainnet";

		public const string TestnetPrefix	= "Testnet";

		public static bool IsTest(string name)	=> name.StartsWith(TestnetPrefix);

		public static ushort Port(string zone)
		{
			if(zone == UC.Net.Zone.Mainnet || zone == UC.Net.Zone.Localnet)
				return 3080;
			else if(IsTest(zone))
				return (ushort)(30800 + ushort.Parse(zone.Substring(TestnetPrefix.Length)));
			else
				throw new IntegrityException("Unknown zone");
		}
	
		public static ushort RpcPort(string zone) => (ushort)(Port(zone) == 3080 ? 3090 : Port(zone) + 100);

	}
}
