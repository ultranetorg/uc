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
			//Chain = x.Get<string>("Chain");
			Provider = x.Get<string>("Provider");
		}
	}

	public class McvSettings
	{
		public int			PeersMin;

		public McvSettings()
		{
		}

		public McvSettings(Xon x)
		{
			PeersMin	= x.Get<int>("PeersMin");
		}
	}

	public class HubSettings
	{
		public HubSettings()
		{
		}

		public HubSettings(Xon x)
		{
		}
	}

	public class FilebaseSettings
	{
		public FilebaseSettings()
		{
		}

		public FilebaseSettings(Xon x)
		{
		}
	}

	public class ApiSettings
	{
		public string	AccessKey;
		Settings		Main;

		public ApiSettings()
		{
		}

		public ApiSettings(Xon x, Settings main)
		{
			Main		= main;
			AccessKey	= x.Get<string>("AccessKey");
		}
	}

	public class SecretSettings
	{
		public const string		FileName = "Secrets.globals";

		public string			Password;
		public string			EthereumWallet;
		public string			EthereumPassword;

		public string			EthereumProvider;

		string					Path;

		public SecretSettings()
		{
		}

		public SecretSettings(string path)
		{
			Path = path;

			var d = new XonDocument(File.ReadAllText(path));
			
			Password			= d.Get<string>("Password");

			EthereumProvider	= d.Get<string>("NasProvider");
			EthereumWallet		= d.Get<string>("EmissionWallet");
			EthereumPassword	= d.Get<string>("EmissionPassword");
		}
	}

	public class DevSettings
	{
		public static bool				UI;
		public static bool				DisableTimeouts;
		public static bool				ThrowOnCorrupted;
		public static bool				SkipSynchronizetion;
		public static bool				SkipDomainVerification;

		public static bool				Any => Fields.Any(i => (bool)i.GetValue(null));
		static IEnumerable<FieldInfo>	Fields => typeof(DevSettings).GetFields().Where(i => i.FieldType == typeof(bool));

		public static string			AsString => string.Join(' ', Fields.Select(i => (bool)i.GetValue(null) ? i.Name : null));

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

	}

	public class Settings
	{
		public const string				FileName = "Sun.settings";

		string							Path; 
		public string					Profile;

		public string					FuiRoles;
		public bool						Log;
		public int						PeersMin = 6;
		public int						PeersInMax = 128;
		public bool						PeersInitialRandomization = true;
		public IPAddress				IP;
		//public IPAddress				ExternalIP;
		public ushort					JsonServerPort;
		public bool						Anonymous = false;
		public List<AccountKey>			Generators = new();
		public AccountKey				Analyzer;
		public string					Packages;

		public NasSettings				Nas;
		public HubSettings				Hub;
		public ApiSettings				Api;
		public FilebaseSettings			Filebase;
		public McvSettings				Mcv;
		public SecretSettings			Secrets;

		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();
		public List<AccountAddress>		ProposedAnalyzerJoiners = new();
		public List<AccountAddress>		ProposedAnalyzerLeavers = new();

		public Money					TransactionPerByteMinFee = new Money(0.000_001);
		public int						TransactionCountPerRoundThreshold = int.MaxValue;

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

			var doc = new XonDocument(File.ReadAllText(Path));

			FuiRoles					= doc.Get<string>("FuiRoles");
			Anonymous					= doc.Has("Anonymous");
			PeersMin					= doc.Get<int>("PeersMin");
			PeersInMax					= doc.Get<int>("PeersInMax");
			PeersInitialRandomization	= doc.Has("PeersInitialRandomization");
			IP							= IPAddress.Parse(doc.Get<string>("IP"));
			//ExternalIP				= IPAddress.Parse(doc.Get<string>("ExternalIP"));
			JsonServerPort				= (ushort)doc.Get<int>("JsonServerPort");
			Generators					= doc.Many("Generator").Select(i => AccountKey.Parse(i.Value as string)).ToList();
			Log							= doc.Has("Log");
			Packages					= doc.GetOrDefault<string>("Packages") ?? System.IO.Path.Join(Profile, "Packages");

			Mcv			= new (doc.One(nameof(Mcv)));
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
			Path		= System.IO.Path.Join(profile, FileName);
			IP			= IPAddress.Loopback;

			Mcv	= new ();
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
