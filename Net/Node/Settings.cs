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

	public class RdsSettings
	{
		public int						PeersMin;
		public int						PeersPermanentMin = 6;
		public Money					Bail;
		public List<AccountAddress>		Generators = new();
		public string					GoogleSearchEngineID;
		public string					GoogleApiKey;
		public List<AccountAddress>		ProposedFundJoiners = new();
		public List<AccountAddress>		ProposedFundLeavers = new();
		public string					Packages;
		public string					Releases;

		public NasSettings				Nas;
		public SeedHubSettings			SeedHub;
		public ResourceHubSettings		ResourceHub;
		public Role						Roles;

		public RdsSettings()
		{
			Nas			= new ();
			SeedHub		= new ();
			ResourceHub	= new ();
		}

		public RdsSettings(Xon x, string profile)
		{
			Roles					= x.Get<Role>("Roles");
			PeersMin				= x.Get<int>("PeersMin");
			PeersPermanentMin		= x.Get<int>("PeersPermanentMin");
			Bail					= x.Get("Bail", Money.Zero);
			Generators				= x.Many("Generator").Select(i => AccountAddress.Parse(i.Value as string)).ToList();
			GoogleSearchEngineID	= x.Get<string>("GoogleSearchEngineID", null);
			GoogleApiKey			= x.Get<string>("GoogleApiKey", null);
			Packages				= x.Get("Packages", Path.Join(profile, "Packages"));
			Releases				= x.Get("Releases", Path.Join(profile, "Releases"));

			Nas			= new (x.One(nameof(Nas)));
			SeedHub		= new (x.One(nameof(SeedHub)));
			ResourceHub	= new (x.One(nameof(ResourceHub)));
		}

		public RdsSettings Merge(Xon x)
		{
			var s = new RdsSettings();

			s.Roles					= Roles;				
			s.PeersMin				= PeersMin;			
			s.PeersPermanentMin		= PeersPermanentMin;	
			s.Bail					= Bail;
			s.Generators			= Generators;
			s.GoogleSearchEngineID	= GoogleSearchEngineID;
			s.GoogleApiKey			= GoogleApiKey;
			s.Packages				= Packages;
			s.Releases				= Releases;

			s.Nas					= Nas;
			s.SeedHub				= SeedHub;
			s.ResourceHub			= ResourceHub;

			if(x.Has("Roles"))
				s.Roles = x.Get<Role>("Roles");

			return s;
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

	public class Settings
	{
		public const string				FileName = "Sun.settings";

		string							Path; 
		public string					Profile;

		public IPAddress				IP;
		public string					FuiRoles;
		public bool						Log;
		public int						PeersPermanentInboundMax = 128;
		public int						PeersInboundMax = 16 * 1024;
		public bool						PeersInitialRandomization = true;
		public string					JsonServerListenAddress;
		public AccountKey				Analyzer;

		public ApiSettings				Api;
		public RdsSettings				Rds;
		public SecretSettings			Secrets;

		public int						RdcQueryTimeout;
		public int						RdcTransactingTimeout;



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
			PeersPermanentInboundMax	= doc.Get<int>("PeersPermanentInboundMax");
			PeersInitialRandomization	= doc.Has("PeersInitialRandomization");
			IP							= doc.Get<IPAddress>("IP", null);
			JsonServerListenAddress		= doc.Get<string>("JsonServerListenAddress", null);
			Log							= doc.Has("Log");

			if(Debugger.IsAttached)
			{
				RdcQueryTimeout = int.MaxValue;
				RdcTransactingTimeout = int.MaxValue;
			}
			else
			{
				RdcQueryTimeout			= doc.Get("RdcQueryTimeout", 5000);
				RdcTransactingTimeout	= doc.Get("RdcTransactingTimeout", 5*60*1000);
			}

			Rds	= new (doc.One(nameof(Rds)), Profile);
			Api	= new (doc.One(nameof(Api)));

			if(boot.Secrets != null)	
				LoadSecrets(boot.Secrets);
		}

		public Settings(string profile, Zone zone)
		{
			Directory.CreateDirectory(profile);

			Profile		= profile;
			Path		= System.IO.Path.Join(profile, FileName);
			IP			= IPAddress.Loopback;

			Rds			= new ();
			Api			= new ();
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
			foreach(var i in Suns.SelectMany(i => i.Mcvs).DistinctBy(i => i.Guid))
			{
				CompareBase(i, Path.Join(destibation, i.GetType().Name));
			}
		}

		public static void CompareBase(Mcv mcv, string destibation)
		{
			//Suns.GroupBy(s => s.Mcv.Accounts.SuperClusters.SelectMany(i => i.Value), Bytes.EqualityComparer);

			var jo = new JsonSerializerOptions(ApiJsonClient.DefaultOptions);
			jo.WriteIndented = true;

			foreach(var i in Suns)
				Monitor.Enter(i.Lock);

			void compare(int table)
			{
				var cs = Suns.Where(i => i.FindMcv(mcv.Guid) != null).Select(i => new {s = i, c = i.FindMcv(mcv.Guid).Tables[table].Clusters.OrderBy(i => i.Id, Bytes.Comparer).ToArray().AsEnumerable().GetEnumerator()}).ToArray();
	
				while(true)
				{
					var x = new bool[cs.Length];

					for(int i=0; i<cs.Length; i++)
						x[i] = cs[i].c.MoveNext();

					if(x.All(i => !i))
						break;
					else if(!x.All(i => i))
						Debugger.Break();
	
					var es = cs.Select(i => new {i.s, e = i.c.Current.BaseEntries.OrderBy(i => i.Id.Ei).ToArray().AsEnumerable().GetEnumerator()}).ToArray();
	
					while(true)
					{
						var y = new bool[es.Length];

						for(int i=0; i<es.Length; i++)
							y[i] = es[i].e.MoveNext();
	
						if(y.All(i => !i))
							break;
						else if(!y.All(i => i))
							Debugger.Break();
	
						var jes = es.Select(i => new {i.s, j = JsonSerializer.Serialize(i.e.Current, jo)}).GroupBy(i => i.j);

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

			foreach(var t in mcv.Tables)
				compare(t.Id);

			foreach(var i in Suns)
				Monitor.Exit(i.Lock);
		}
	}
}
