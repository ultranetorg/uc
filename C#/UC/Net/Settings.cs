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
	public class DatabaseSettings
	{
		//public bool			Enabled;

		public DatabaseSettings(Xon x)
		{
			//Enabled	= x.One("Enabled") != null;
		}
	}

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

	public class HubSettings
	{
		public bool Enabled;

		public HubSettings(Xon x)
		{
			Enabled = x.Has("Enabled");
		}
	}

	public class RpcSettings
	{
		public bool		Enabled = true;
		//public int	Port { get; }
		public string	AccessKey;
		Settings		Main;

		public RpcSettings(Xon x, Settings main)
		{
			Main		= main;
			Enabled		= x.One("Enabled") != null;
			//Port		= x.Has("Port") ? x.GetInt32("Port") : DefaultPort;
			AccessKey	= x.GetString("AccessKey");
		}
	}

	public class SecretSettings
	{
		public string		Password;
		public string		EmissionWallet;
		public string		EmissionPassword;

		public string		NasWallet;
		public string		NasPassword;

		public string		Path;
		public string		Fathers => System.IO.Path.Join(Path, "Fathers");	

		public SecretSettings(string path)
		{
			Path = path;

			var s = System.IO.Path.Join(path, "Secrets.xon");

			var d = new XonDocument(new XonTextReader(File.ReadAllText(s)), XonTextValueSerializator.Default);
				
			Password	= d.GetString("Password");

			NasWallet		= d.GetString("NasWallet");
			NasPassword		= d.GetString("NasPassword");

			EmissionWallet	 = d.GetString("EmissionWallet");
			EmissionPassword = d.GetString("EmissionPassword");
		}
	}

	public class DevSettings
	{
		public bool UI;
		public bool GenerateGenesis;
		public bool DisableBailMin;
		public bool DisableBidMin;
		public bool DisableTimeouts;
		public bool ThrowOnCorrupted;

		public bool Any => Fields.Any(i => (bool)i.GetValue(this));
		IEnumerable<FieldInfo> Fields => GetType().GetFields().Where(i => i.FieldType == typeof(bool));

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
		public bool					Log;
		public int					Port;
		public string				Zone;
		public int					PeersMin;
		public int					PeersInMax;
		public IPAddress			IP = IPAddress.Any;
		public string				Generator;
		public string				Profile;

		public static DevSettings	Dev;
		public NasSettings			Nas;
		public HubSettings			Hub;
		public RpcSettings			Api;
		public DatabaseSettings		Database;
		public SecretSettings		Secret;

		public List<string>			Accounts;
		public List<Account>		ApprovedFundables = new();

		XonDocument					Document;
		string						Path; 

		public Cryptography Cryptography
		{
			get
			{
				if(Zone == UC.Net.Zone.Localnet)
					return new NoCryptography();
				else if(Zone == UC.Net.Zone.Mainnet || UC.Net.Zone.IsTest(Zone))
					return new EthereumCryptography();
				else
					throw new IntegrityException("Unknown zone");
			}
		}

		public Settings()
		{
		}

		public Settings(BootArguments boot)
		{
			Path			= System.IO.Path.Join(boot.Main.Profile, "Settings.xon");
			Zone			= boot.Main.Zone;

			var doc = new XonDocument(new XonTextReader(File.ReadAllText(Path)), XonTextValueSerializator.Default);

			PeersMin	= doc.GetInt32("PeersMin");
			PeersInMax	= doc.GetInt32("PeersInMax");
			Port		= doc.Has("Port") ? doc.GetInt32("Port") : UC.Net.Zone.Port(Zone);
			IP			= IPAddress.Parse(doc.GetString("IP"));
			Generator	= doc.Has("Generator") ? doc.GetString("Generator") : null;
			Log			= doc.Has("Log");

			Dev			= new (doc.One(nameof(Dev)));
			Database	= new (doc.One(nameof(Database)));
			Nas			= new (doc.One(nameof(Nas)));
			Hub			= new (doc.One(nameof(Hub)));
			Api			= new (doc.One(nameof(Api)), this);

			if(doc.Has("Secrets") && File.Exists(doc.GetString("Secrets")))
			{
				LoadSecrets(doc.GetString("Secrets"));
			}

			if(boot.Core.Port != -1)	Port = boot.Core.Port;
			if(boot.Core.IP != null)	IP = boot.Core.IP;

			if(boot.Main.Profile != null)	Profile = boot.Main.Profile;
			if(boot.Main.Secrets != null)	LoadSecrets(boot.Main.Secrets);

			if(boot.Vault.Accounts.Any())	Accounts = boot.Vault.Accounts;

			if(boot.Api.AccessKey != null)	Api.AccessKey = boot.Api.AccessKey;

			Document = doc;
		}

		void LoadSecrets(string path)
		{
			Secret = new SecretSettings(path);
		}

		public void Save()
		{
			void save(Xon xon, object o, FieldInfo fi)
			{
				void set(object v)
				{
					(xon.One(fi.Name) ?? xon.Add(fi.Name)).Value = v;
				}

				if(fi.Name == nameof(Profile) || fi.Name == nameof(Secret))
					return;

				if(fi.FieldType.Name.EndsWith("Settings"))
				{
					foreach(var j in fi.FieldType.GetFields())
					{
						save(xon.One(fi.Name), fi.GetValue(o), j);
					}
				}
				else
					if(fi.FieldType == typeof(Int32))	set((int)fi.GetValue(o)); else
					if(fi.FieldType == typeof(String))	set(fi.GetValue(o) as string); else 
					if(fi.FieldType == typeof(Boolean))	{ 
															var x = xon.One(fi.Name);

															if((bool)fi.GetValue(o))
															{
																if(x == null)
																	xon.Add(fi.Name);
															}
															else if(x != null)
																xon.Nodes.Remove(x);
														}
			}

			foreach(var i in GetType().GetFields())
			{
				save(Document, this, i);
			}


			using(var s = File.Create(Path))
			{
				Document.Save(new XonTextWriter(s, Encoding.UTF8));
			}
		}
	}
}
