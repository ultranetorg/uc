using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Nethereum.Util;
using System.Reflection;

namespace UC.Net
{
	public class NasSettings
	{
		public string		Chain;
		public string		Provider;

		public NasSettings(Xon x)
		{
			Chain = x.GetString("Chain");
			Provider = x.GetString("Provider");
		}
	}

	public class DatabaseSettings
	{
		public bool			Base;
		public bool			Chain;
		public int			PeersMin;

		public DatabaseSettings(Xon x)
		{
			Chain		= x.Has("Chain");
			Base		= x.Has("Base");
			PeersMin	= x.GetInt32("PeersMin");
		}
	}

	public class HubSettings
	{
		public bool Enabled;

		public HubSettings(Xon x)
		{
			Enabled = x.Has("Enabled");
		}
	}

	public class FilebaseSettings
	{
		public bool Enabled;

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
		public const string FileName = "Secrets.globals";

		public string			Password;
		public string			EmissionWallet;
		public string			EmissionPassword;

		public string			NasWallet;
		public string			NasPassword;
		public string			NasProvider;

		public string			Path;
		string					FathersPath => System.IO.Path.Join(Path, "Fathers");	
		PrivateAccount[]		_Fathers;
		PrivateAccount			_OrgAccount;
		PrivateAccount			_GenAccount;

		public PrivateAccount OrgAccount
		{
			get
			{
				if(_OrgAccount == null)
					_OrgAccount = PrivateAccount.Load(System.IO.Path.Join(Path, "0xeeee974ab6b3e9533ee99f306460cfc24adcdae0." + Vault.NoCryptoWalletExtention), null);

				return _OrgAccount;
			}
		}

		public PrivateAccount GenAccount
		{
			get
			{
				if(_GenAccount == null)
					_GenAccount = PrivateAccount.Load(System.IO.Path.Join(Path, "0xffff50e1605b6f302850694291eb0e688ef15677." + Vault.NoCryptoWalletExtention), null);

				return _GenAccount;
			}
		}

		public PrivateAccount[] Fathers
		{
			get
			{
				if(_Fathers == null)
					_Fathers = Directory.EnumerateFiles(FathersPath, "*." + Vault.NoCryptoWalletExtention).Select(i => PrivateAccount.Load(i, null)).OrderBy(i => i).ToArray();

				return _Fathers;
			}
		}

		public SecretSettings(string path)
		{
			Path = path;

			var s = System.IO.Path.Join(path, FileName);

			var d = new XonDocument(new XonTextReader(File.ReadAllText(s)), XonTextValueSerializator.Default);
				
			Password	= d.GetString("Password");

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
		public int				TailLength = 100;

		public bool				Any => Fields.Any(i => (bool)i.GetValue(this));
		IEnumerable<FieldInfo>	Fields => GetType().GetFields().Where(i => i.FieldType == typeof(bool));

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
		public const string			FileName = "Settings.xon";

		string						Path; 

		public readonly int			Port;
		public readonly Zone		Zone;
	
		public bool					Log;
		public int					PeersMin;
		public int					PeersInMax;
		public IPAddress			IP = IPAddress.Any;
		public PrivateAccount[]		Generators;
		public string				Profile;

		public static DevSettings	Dev;
		public NasSettings			Nas;
		public HubSettings			Hub;
		public ApiSettings			Api;
		public FilebaseSettings		Filebase;
		public DatabaseSettings		Database;
		public SecretSettings		Secret;

		public List<Account>		ProposedFundables = new(){};

		public Cryptography Cryptography
		{
			get
			{
				if(Zone == UC.Net.Zone.Localnet)
					return new NoCryptography();
				else if(Zone == UC.Net.Zone.Mainnet || Zone.IsTest)
					return new EthereumCryptography();
				else
					throw new IntegrityException("Unknown zone");
			}
		}

		public Settings()
		{
		}

		public Settings(string exedir, BootArguments boot)
		{
			Directory.CreateDirectory(boot.Profile);

			var orig = System.IO.Path.Join(exedir, FileName);
			Path = System.IO.Path.Join(boot.Profile, FileName);

			if(!File.Exists(Path))
			{
				File.Copy(orig, Path, true);
			}

			Profile = boot.Profile;
			Zone	= Zone.All.First(i => i.Name == boot.Zone);

			var doc = new XonDocument(new XonTextReader(File.ReadAllText(Path)), XonTextValueSerializator.Default);

			PeersMin	= doc.GetInt32("PeersMin");
			PeersInMax	= doc.GetInt32("PeersInMax");
			Port		= doc.Has("Port") ? doc.GetInt32("Port") : Zone.Port;
			IP			= IPAddress.Parse(doc.GetString("IP"));
			Generators	= doc.Many("Generator").Select(i => PrivateAccount.Parse(i.Value as string)).ToArray();
			Log			= doc.Has("Log");

			Dev			= new (doc.One(nameof(Dev)));
			Database	= new (doc.One(nameof(Database)));
			Nas			= new (doc.One(nameof(Nas)));
			Api			= new (doc.One(nameof(Api)), this);
			Hub			= new (doc.One(nameof(Hub)));
			Filebase	= new (doc.One(nameof(Filebase)));

			if(doc.Has("Secrets") && File.Exists(doc.GetString("Secrets")))
			{
				LoadSecrets(doc.GetString("Secrets"));
			}
						
			if(boot.Secrets != null)	LoadSecrets(boot.Secrets);
		}

		void LoadSecrets(string path)
		{
			Secret = new SecretSettings(path);
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
				if(i.Name == nameof(Profile) || i.Name == nameof(Secret))
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
