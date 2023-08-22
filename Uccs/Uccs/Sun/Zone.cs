using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Nethereum.Signer;

namespace Uccs.Net
{
	public class Zone
	{
		public string			Name;
		public Chain			EthereumNetwork;
		public const string		TestnetPrefix	= "Testnet";
		public Cryptography		Cryptography;
		public string			Genesis;
		public IPAddress		GenesisIP;
		public IPAddress[]		Initials;
		public ZoneCreation		Creation;
		public Coin				BailMin;
		public bool				PoW;
		public bool				CheckDomains;

		public AccountAddress	OrgAccount = AccountAddress.Parse("0xeeee974ab6b3e9533ee99f306460cfc24adcdae0");
		public AccountAddress	GenAccount = AccountAddress.Parse("0xffff50e1605b6f302850694291eb0e688ef15677");
		public AccountAddress	Father0 = AccountAddress.Parse("0x000038a7a3cb80ec769c632b7b3e43525547ecd1");

		public ushort Port	{
								get
								{
									if(IsTest)
										return (ushort)(30800 + ushort.Parse(Name.Substring(TestnetPrefix.Length)));
									else
										return 3080;
								}
							}
	
		public ushort JsonPort => (ushort)(IsTest ? (Port + 100) : 3090);

 		public static readonly Zone		Testnet1 = new Testnet1();
		public static readonly Zone		Mainnet	= null;
		public static readonly Zone[]	Official = {Testnet1, Mainnet};

		public Zone()
		{
		}

		public override string ToString()
		{
			return Name;
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

			Genesis			= d.GetString("Genesis");
			Cryptography	= Activator.CreateInstance(typeof(NoCryptography).Assembly.FullName, typeof(NoCryptography).Namespace + '.' + d.GetString("Cryptography")).Unwrap() as Uccs.Net.Cryptography;
			EthereumNetwork	= Enum.Parse<Chain>(d.GetString("EthereumNetwork"));
			GenesisIP		= IPAddress.Parse(d.GetString("GenesisIP"));
			Initials		= d.GetString("Initials").Split(' ').Select(i => IPAddress.Parse(i)).ToArray();
		}
	}
}
