using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Nethereum.Signer;

namespace Uccs.Net
{
	public class Zone : ICloneable
	{
		public const string		TestnetPrefix	= "Testnet";

		public string			Name;
		public string			Genesis;	
		public IPAddress		GenesisIP;
		public IPAddress[]		Initials;
 		public Cryptography		Cryptography									= Cryptography.Ethereum;
 		public Chain			EthereumNetwork									= Chain.Goerli;	
		public bool				PoW												= true;
		public int				TailLength										= 1000;
		public int				ExternalVerificationDurationLimit				= 100;
		public Money			BailMin											= 1000;
		public int				MembersLimit									= 1000;
		public int				TransactionsPerRoundLimit						= 5_000;
		public int				TransactionsPerVoteAllowableOverflowMuliplier	= 10;
		public int				TransactionsOverflowFactor						= 10;
		public int				OperationsPerTransactionLimit					= 100;
		public int				OperationsPerRoundLimit							=> TransactionsPerRoundLimit * OperationsPerTransactionLimit;
		public Money			ExeunitMinFee									= new Money(0.000_001);	/// 50 *1000 / (1e7/100*0.2*3600*24*30) = 0.000 000 192
																										/// 50  - server per month avarage cost in usd
																										/// 1000 - members max limit
																										///	1e7 - 100 MB/s bandwidth
																										///	100 - avarage length of operation
																										///	0.2 - effective bandwidth utilization 

		public AccountAddress	God = AccountAddress.Parse("0xffff50e1605b6f302850694291eb0e688ef15677");
		public AccountAddress	Father0 = AccountAddress.Parse("0x000038a7a3cb80ec769c632b7b3e43525547ecd1");

		public ushort Port	{
								get
								{
									if(IsTest)
										return (ushort)(3800 + ushort.Parse(Name.Substring(TestnetPrefix.Length)));
									else
										return 3800;
								}
							}

 		public static readonly Zone		Testnet1 = new Testnet1();
 		public static readonly Zone		Simulation = new SimulationZone();
		public static readonly Zone		Mainnet	= null;
		public static readonly Zone[]	Official = {Simulation, Testnet1};

		public Zone()
		{
		}

		public override string ToString()
		{
			return Name;
		}

		public object Clone()
		{
			var z = new Zone();

			z.Name = 			Name;
			z.EthereumNetwork = EthereumNetwork;
			z.Cryptography = 	Cryptography;
			z.Genesis = 		Genesis;
			z.GenesisIP = 		GenesisIP;
			z.Initials = 		Initials;
			z.BailMin = 		BailMin;
			z.PoW = 			PoW;		
			z.TailLength = 		TailLength;
			
			z.God = 			God;
			z.Father0 = 		Father0;
			
			return z;
		}

		public static Zone OfficialByName(string name) => Official.First(i => i.Name == name);

		public bool IsTest	=> Name.StartsWith(TestnetPrefix);

		public void Save(string directory)
		{
			var d = new XonDocument(new XonTextValueSerializator());

			d.Add("Genesis").Value			= Genesis;
			d.Add("Cryptography").Value		= Cryptography.GetType().Name;
			d.Add("EthereumNetwork").Value	= EthereumNetwork.ToString();
			d.Add("GenesisIP").Value		= GenesisIP.ToString();
			d.Add("Initials").Value			= string.Join(' ', Initials.AsEnumerable());

			using(var s = File.Create(Path.Join(directory, Name + ".zone")))
				d.Save(new XonTextWriter(s, Encoding.UTF8));
		}

		public void Load(string path)
		{
			var d = new XonDocument(File.ReadAllText(path));

			Name = Path.GetFileNameWithoutExtension(path);

			Genesis			= d.Get<string>("Genesis");
			Cryptography	= Activator.CreateInstance(typeof(NoCryptography).Assembly.FullName, typeof(NoCryptography).Namespace + '.' + d.Get<string>("Cryptography")).Unwrap() as Uccs.Net.Cryptography;
			EthereumNetwork	= Enum.Parse<Chain>(d.Get<string>("EthereumNetwork"));
			GenesisIP		= IPAddress.Parse(d.Get<string>("GenesisIP"));
			Initials		= d.Get<string>("Initials").Split(' ').Select(i => IPAddress.Parse(i)).ToArray();
		}
	}
}
