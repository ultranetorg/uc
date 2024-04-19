using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;

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
		public int		PeersMin;

		public McvSettings()
		{
		}

		public McvSettings(Xon x)
		{
			PeersMin = x.Get<int>("PeersMin");
		}
	}

	public class SeedHubSettings
	{
		public SeedHubSettings()
		{
		}

		public SeedHubSettings(Xon x)
		{
		}
	}

	public class ResourceHubSettings
	{
		public int		CollectRefreshInterval = 60000;

		public ResourceHubSettings()
		{
		}

		public ResourceHubSettings(Xon x)
		{
		}
	}

	public class ApiSettings
	{
		public string	AccessKey;

		public ApiSettings()
		{
		}

		public ApiSettings(Xon x)
		{
			AccessKey = x.Get<string>("AccessKey");
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

	public class SunGlobals
	{
		public static bool				UI;
		public static bool				DisableTimeouts;
		public static bool				ThrowOnCorrupted;
		public static bool				SkipSynchronization;
		public static bool				SkipMigrationVerification;

		public static bool				Any => Fields.Any(i => (bool)i.GetValue(null));
		static IEnumerable<FieldInfo>	Fields => typeof(SunGlobals).GetFields().Where(i => i.FieldType == typeof(bool));

		public static string			AsString => string.Join(' ', Fields.Select(i => (bool)i.GetValue(null) ? i.Name : null));

		public static List<Sun>			Suns = new();

		public SunGlobals()
		{
		}

		public SunGlobals(Xon x)
		{
			if(x != null)
			{
				foreach(var i in Fields)
				{
					i.SetValue(this, x.Has(i.Name));
				}
			}
		}

		public static void CompareBase(string destibation)
		{
			//Suns.GroupBy(s => s.Mcv.Accounts.SuperClusters.SelectMany(i => i.Value), Bytes.EqualityComparer);

			var jo = new JsonSerializerOptions(SunJsonApiClient.DefaultOptions);
			jo.WriteIndented = true;

			foreach(var i in Suns)
				Monitor.Enter(i.Lock);

			void compare<E, K>(Func<Sun, Table<E, K>> get) where E : class, ITableEntry<K>
			{
				var cs = Suns.Where(i => i.Mcv != null).Select(i => new {s = i, c = get(i).Clusters.OrderBy(i => i.Id, Bytes.Comparer).ToArray().AsEnumerable().GetEnumerator() }).ToArray();
	
				while(true)
				{
					var x = new bool[cs.Length];

					for(int i=0; i<cs.Length; i++)
						x[i] = cs[i].c.MoveNext();

					if(x.All(i => !i))
						break;
					else if(!x.All(i => i))
						Debugger.Break();
	
					var es = cs.Select(i => new {i.s, e = i.c.Current.Entries.OrderBy(i => i.Id.Ei).ToArray().AsEnumerable().GetEnumerator()}).ToArray();
	
					while(true)
					{
						var y = new bool[es.Length];

						for(int i=0; i<es.Length; i++)
							y[i] = es[i].e.MoveNext();
	
						if(y.All(i => !i))
							break;
						else if(!y.All(i => i))
							Debugger.Break();
	
						var jes = es.Select(i => new {i.s, j = JsonSerializer.Serialize(i.e.Current, jo) }).GroupBy(i => i.j);

						if(jes.Count() > 1)
						{
							foreach(var i in jes)
							{
								File.WriteAllText(Path.Join(destibation, string.Join(',', i.Select(i => i.s.Settings.IP.GetAddressBytes()[3].ToString()))), i.Key);
							}
							
							Debugger.Break();
						}
					}
				}
			}

			compare(i => i.Mcv.Accounts);
			compare(i => i.Mcv.Authors);

			foreach(var i in Suns)
				Monitor.Exit(i.Lock);
		}
	}

	public class Settings
	{
		public const string				FileName = "Sun.settings";

		string							Path; 
		public string					Profile;

		public string					FuiRoles;
		public bool						Log;
		public int						PeersPermanentMin = 6;
		public int						PeersPermanentInboundMax = 128;
		public int						PeersInboundMax = 16 * 1024;
		public bool						PeersInitialRandomization = true;
		public IPAddress				IP;
		public Money					Bail;
		public string					JsonServerListenAddress;
		public List<AccountAddress>		Generators = new();
		public AccountKey				Analyzer;
		public string					Packages;

		public ApiSettings				Api;
		public McvSettings				Mcv;
		public NasSettings				Nas;
		public SeedHubSettings			SeedHub;
		public ResourceHubSettings		ResourceHub;
		public SecretSettings			Secrets;
		public string					GoogleSearchEngineID;
		public string					GoogleApiKey;

		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();
		public List<AccountAddress>		ProposedAnalyzerJoiners = new();
		public List<AccountAddress>		ProposedAnalyzerLeavers = new();

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

			var doc = new XonDocument(File.ReadAllText(Path), SunXonTextValueSerializator.Default);

			FuiRoles					= doc.Get<string>("FuiRoles");
			PeersPermanentMin			= doc.Get<int>("PeersPermanentMin");
			PeersPermanentInboundMax	= doc.Get<int>("PeersPermanentInboundMax");
			PeersInitialRandomization	= doc.Has("PeersInitialRandomization");
			IP							= doc.Has("IP") ? IPAddress.Parse(doc.Get<string>("IP")) : null;
			JsonServerListenAddress		= doc.Get<string>("JsonServerListenAddress", null);
			Bail						= doc.Get("Bail", Money.Zero);
			Generators					= doc.Many("Generator").Select(i => AccountAddress.Parse(i.Value as string)).ToList();
			Log							= doc.Has("Log");
			Packages					= doc.Get("Packages", System.IO.Path.Join(Profile, "Packages"));

			GoogleSearchEngineID		= doc.Get<string>("GoogleSearchEngineID", null);
			GoogleApiKey				= doc.Get<string>("GoogleApiKey", null);

			Mcv			= new (doc.One(nameof(Mcv)));
			Nas			= new (doc.One(nameof(Nas)));
			Api			= new (doc.One(nameof(Api)));
			SeedHub		= new (doc.One(nameof(SeedHub)));
			ResourceHub	= new (doc.One(nameof(ResourceHub)));

			if(boot.Secrets != null)	
				LoadSecrets(boot.Secrets);
		}

		public Settings(string profile, Zone zone)
		{
			Directory.CreateDirectory(profile);

			Profile		= profile;
			Path		= System.IO.Path.Join(profile, FileName);
			IP			= IPAddress.Loopback;

			Mcv			= new ();
			Nas			= new ();
			Api			= new ();
			SeedHub		= new ();
			ResourceHub	= new ();
		}

		public void LoadSecrets(string path)
		{
			Secrets = new SecretSettings(path);
		}

		public XonDocument Save()
		{
			var doc = new XonDocument(SunXonTextValueSerializator.Default);

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
					if(type == typeof(bool))	
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
