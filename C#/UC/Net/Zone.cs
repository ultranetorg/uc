using System;
using System.Linq;
using System.Net;
using Nethereum.Signer;

namespace UC.Net
{
	public class Zone
	{
		public readonly string	Name;
		public readonly Chain	EtheterumNetwork;
		public const int 		GeneratorsPerIP = 8;

		public static readonly Zone Localnet = new Zone("Localnet",			 Chain.Goerli){Initials = Enumerable.Range(100, 8).Select(i => new IPAddress(new byte[]{192, 168, 1, (byte)i})).ToArray()};
		public static readonly Zone Testnet1 = new Zone(TestnetPrefix + "1", Chain.Goerli){Initials = new string[]{	"168.119.54.200",
																													"78.47.204.100", 
																													"78.47.214.161",
																													"78.47.214.166",
																													"78.47.214.170",
																													"78.47.214.171",
																													"78.47.198.218",
																													"78.47.205.229"}.Select(i => IPAddress.Parse(i)).ToArray()};

		public static readonly Zone Mainnet	= new Zone("Mainnet", Chain.MainNet);
		public static readonly Zone[]	All = {Localnet, Testnet1, Mainnet};

		public const string TestnetPrefix	= "Testnet";

		public IPAddress[] Initials;
												

		public Zone(string name, Chain etheterumNetwork)
		{
			Name = name;
			EtheterumNetwork = etheterumNetwork;
		}

		public override string ToString()
		{
			return Name;
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
