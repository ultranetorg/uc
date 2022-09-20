using System;
using System.Linq;

namespace UC.Net
{
	public class Zone
	{
		public readonly string Name;
		public readonly string EtheterumNetwork;

		public static readonly Zone Localnet	= new Zone("Localnet",			"Ropsten");
		public static readonly Zone Testnet0	= new Zone(TestnetPrefix + "0", "Ropsten");
		public static readonly Zone Testnet1	= new Zone(TestnetPrefix + "1", "Ropsten");
		public static readonly Zone Mainnet		= new Zone("Mainnet",			"Mainnet");

		public static readonly Zone[] All = {Localnet, Testnet0, Testnet1, Mainnet};

		public const string TestnetPrefix	= "Testnet";

		public Zone(string name, string etheterumNetwork)
		{
			Name = name;
			EtheterumNetwork = etheterumNetwork;
		}

		public static Zone ByName(string name) => All.First(i => i.Name == name);

		public bool IsTest	=> Name.StartsWith(TestnetPrefix);

		public ushort Port	{
								get
								{
									if(Name == Mainnet.Name || Name == Localnet.Name)
										return 3080;
									else if(IsTest)
										return (ushort)(30800 + ushort.Parse(Name.Substring(TestnetPrefix.Length)));
									else
										throw new IntegrityException("Unknown zone");

								}
							}
	
		public ushort JsonPort => (ushort)((Port) == 3080 ? 3090 : (Port + 100));
	}
}
