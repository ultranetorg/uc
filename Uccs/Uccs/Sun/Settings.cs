using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nethereum.Util;
using System.Reflection;
using Nethereum.Signer;

namespace Uccs.Net
{
	public class NasSettings
	{
		//public string		Chain;
		public string		Provider;

		public NasSettings()
		{
		}

		public NasSettings(Xon x)
		{
			//Chain = x.GetString("Chain");
			Provider = x.GetString("Provider");
		}
	}

	public class DatabaseSettings
	{
		public bool			Base;
		public bool			Chain;
		public int			PeersMin;

		public DatabaseSettings()
		{
		}

		public DatabaseSettings(Xon x)
		{
			Base		= x.Has("Base");
			Chain		= x.Has("Chain");
			PeersMin	= x.GetInt32("PeersMin");
		}
	}

	public class HubSettings
	{
		public bool Enabled;

		public HubSettings()
		{
		}

		public HubSettings(Xon x)
		{
			Enabled = x.Has("Enabled");
		}
	}

	public class FilebaseSettings
	{
		public bool Enabled;

		public FilebaseSettings()
		{
		}

		public FilebaseSettings(Xon x)
		{
			Enabled = x.Has("Enabled");
		}
	}

	public class ApiSettings
	{
		public bool		Enabled = true;
		//public int	Port { get; }
		public string	AccessKey;
		Settings		Main;

		public ApiSettings()
		{
		}

		public ApiSettings(Xon x, Settings main)
		{
			Main		= main;
			Enabled		= x.One("Enabled") != null;
			//Port		= x.Has("Port") ? x.GetInt32("Port") : DefaultPort;
			AccessKey	= x.GetString("AccessKey");
		}
	}

	public class SecretSettings
	{
		public const string		FileName = "Secrets.globals";

		public string			Password;
		public string			EmissionWallet;
		public string			EmissionPassword;

		public string			NasWallet;
		public string			NasPassword;
		public string			NasProvider;

		public string			Path;
		string					FathersPath => System.IO.Path.Join(Path, "Fathers");	
		AccountKey[]			_Fathers;
		AccountKey				_OrgAccount;
		AccountKey				_GenAccount;

		public AccountKey OrgAccount
		{
			get
			{
				if(_OrgAccount == null)
				{
					var c = new NoCryptography();
					_OrgAccount = AccountKey.Load(c, System.IO.Path.Join(Path, "0xeeee974ab6b3e9533ee99f306460cfc24adcdae0." + Vault.NoCryptoWalletExtention), null);
				}

				return _OrgAccount;
			}
		}

		public AccountKey GenAccount
		{
			get
			{
				if(_GenAccount == null)
				{ 
					var c = new NoCryptography();
					_GenAccount = AccountKey.Load(c, System.IO.Path.Join(Path, "0xffff50e1605b6f302850694291eb0e688ef15677." + Vault.NoCryptoWalletExtention), null);
				}

				return _GenAccount;
			}
		}

		public AccountKey[] Fathers
		{
			get
			{
				if(_Fathers == null)
				{
					var c = new NoCryptography();
					_Fathers = Directory.EnumerateFiles(FathersPath, "*." + Vault.NoCryptoWalletExtention).Select(i => AccountKey.Load(c , i, null)).OrderBy(i => i).ToArray();
				}

				return _Fathers;
			}
		}

		public SecretSettings(string path)
		{
			Path = path;

			var s = System.IO.Path.Join(path, FileName);

			var d = new XonDocument(File.ReadAllText(s));
			
			Password		= d.GetString("Password");

			NasWallet		= d.GetString("NasWallet");
			NasPassword		= d.GetString("NasPassword");
			NasProvider		= d.GetString("NasProvider");

			EmissionWallet	 = d.GetString("EmissionWallet");
			EmissionPassword = d.GetString("EmissionPassword");
		}
	}

	public class DevSettings
	{
		public bool				UI;
		public bool				GenerateGenesis;
		public bool				DisableBailMin;
		public bool				DisableBidMin;
		public bool				DisableTimeouts;
		public bool				ThrowOnCorrupted;
		public bool				TailLength100;

		public bool				Any => Fields.Any(i => (bool)i.GetValue(this));
		IEnumerable<FieldInfo>	Fields => GetType().GetFields().Where(i => i.FieldType == typeof(bool));

		public DevSettings()
		{
		}

		public DevSettings(Xon x)
		{
			if(x != null)
			{
				foreach(var i in Fields)
				{
					i.SetValue(this, x.Has(i.Name));
				}
			}
		}

		public override string ToString()
		{
			return string.Join(' ', Fields.Select(i => (bool)i.GetValue(this) ? i.Name : null));
		}
	}

	public class Settings
	{
		public const string				FileName = "Sun.settings";

		string							Path; 

		//public readonly int				Port;
		//public readonly Zone			Zone;
	
		public bool						Log;
		public int						PeersMin;
		public int						PeersInMax;
		public IPAddress				IP;
		public List<AccountKey>			Generators = new();
		public string					Profile;
		public string					ProductsPath;

		public static DevSettings		Dev;
		public NasSettings				Nas;
		public HubSettings				Hub;
		public ApiSettings				Api;
		public FilebaseSettings			Filebase;
		public DatabaseSettings			Database;
		public SecretSettings			Secrets;

		public List<AccountAddress>		ProposedFunds = new(){};

// 		public Cryptography Cryptography
// 		{
// 			get
// 			{
// 				if(Zone == Uccs.Net.Zone.Localnet)
// 					return new NoCryptography();
// 				else if(Zone == Uccs.Net.Zone.Mainnet || Zone.IsTest)
// 					return new EthereumCryptography();
// 				else
// 					throw new IntegrityException("Unknown zone");
// 			}
// 		}

		public Settings()
		{
		}

		public Settings(string exedir, Boot boot)
		{
			Directory.CreateDirectory(boot.Profile);

			var orig = System.IO.Path.Join(exedir, FileName);
			Path = System.IO.Path.Join(boot.Profile, FileName);

			if(!File.Exists(Path))
			{
				File.Copy(orig, Path, true);
			}

			Profile = boot.Profile;
			//Zone	= Zone.All.First(i => i.Name == boot.Zone);

			var doc = new XonDocument(File.ReadAllText(Path));

			PeersMin	= doc.GetInt32("PeersMin");
			PeersInMax	= doc.GetInt32("PeersInMax");
			//Port		= doc.GetInt32("Port");
			IP			= IPAddress.Parse(doc.GetString("IP"));
			Generators	= doc.Many("Generator").Select(i => AccountKey.Parse(i.Value as string)).ToList();
			Log			= doc.Has("Log");

			Dev			= new (doc.One(nameof(Dev)));
			Database	= new (doc.One(nameof(Database)));
			Nas			= new (doc.One(nameof(Nas)));
			Api			= new (doc.One(nameof(Api)), this);
			Hub			= new (doc.One(nameof(Hub)));
			Filebase	= new (doc.One(nameof(Filebase)));

			if(boot.Secrets != null)	
				LoadSecrets(boot.Secrets);
		}

		public Settings(string profile, Zone zone)
		{
			Directory.CreateDirectory(profile);

			Profile		= profile;
			//Zone		= zone;
			//Port		= Zone.Port;
			IP			= IPAddress.Loopback;

			Dev			= new ();
			Database	= new ();
			Nas			= new ();
			Api			= new ();
			Hub			= new ();
			Filebase	= new ();
		}

		public void LoadSecrets(string path)
		{
			Secrets = new SecretSettings(path);
		}

		public XonDocument Save()
		{
			var doc = new XonDocument(XonTextValueSerializator.Default);

			void save(Xon parent, string name, Type type, object value)
			{
				if(type.Name.EndsWith("Settings"))
				{
					var x = parent.Add(name);
					///var v = fi.GetValue(owner);

					foreach(var f in type.GetFields().Where(i => !i.IsInitOnly && !i.IsLiteral))
					{
						save(x, f.Name, f.FieldType, f.GetValue(value));
					}
				}
				else
					if(type == typeof(Boolean))	
					{ 
						if((bool)value)
						{
							parent.Add(name);
						}
					}
					else if(type.GetInterfaces().Any(i => i == typeof(System.Collections.IEnumerable) && type.GetGenericArguments().Any()))
					{
						foreach(var i in value as System.Collections.IEnumerable)
						{
							save(parent, name.Trim('s'), type.GetGenericArguments()[0], i);
						}
					}
					else
						parent.Add(name).Value = value;
			}

			foreach(var i in GetType().GetFields().Where(i => !i.IsInitOnly && !i.IsLiteral))
			{
				if(i.Name == nameof(Profile) || i.Name == nameof(Secrets))
					continue;

				save(doc, i.Name, i.FieldType, i.GetValue(this));
			}

			using(var s = File.Create(Path))
			{
				doc.Save(new XonTextWriter(s, Encoding.UTF8));
			}

			return doc;
		}
	}
}
