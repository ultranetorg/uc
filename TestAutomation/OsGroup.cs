using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using O;

using Uccs.Net;
using Uccs.Nexus;
using Uccs;
using System.Threading;

namespace TestAutomation;

public class OsGroup
{
	public const string AdminPassword = "admin_secret";

	public const Zone				Zone = Uccs.Net.Zone.TA;
	public List<Os>					Oses = new();
	public Os						F0;
	public readonly string			ExeDirectory;
	public List<Father>				Fathers = new();
	public Os.Delegate				OsAdded;
	public Os.Delegate				OsRemoved;
	public readonly IPAddress[]		Initials;

	// From Simulation.
	public string WorkPath;
	public Flow Flow;

	public OsGroup(string workpath, IPAddress[] initials)
	{
		WorkPath = workpath;
		Flow = new Flow("TA Simulation", new Log());

		ExeDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		Initials = initials;
	}

	public Father ReserveFather()
	{
		var a = G.Fathers.Keys.First(i => !Fathers.Any(j => j.Key.Puplic == i.Key.Puplic));

		Fathers.Add(a);

		return a;
	}

	public Os CreateOs(string name, string args, IPAddress ip)
	{
		var x = new Xon(args);

		var profile = Path.Join(WorkPath, name);

		IEnumerable<Father> fs = [];

		if(x.Has("f"))
			fs = Enumerable.Range(0, x.Get<int>("f")).Select(i => ReserveFather());

		var os = new Os(name)
		{
			IP = ip,
			Profile = profile
		};
		Oses.Add(os);


		var ns = new NexusSettings(Zone, os.Profile)
		{
			Name = name,
			Host = ip,
			Api = new() { LocalIP = ip },
		};

		var vs = new VaultSettings(ns)
		{
			Api = new() { LocalIP = ip, AdminPasswordHash = Apc.HashifyAdminPassword(AdminPassword) },
			CreateFirstKeyIfEmpty = false
		};
		vs.Save();

		if(x.Has("nn"))
		{
			ns.IccpPeering = new()
			{
				Endpoint = new(ip, Port.Map(Zone, KnownProtocol.Iccp)),
				PermanentMin = 2,
			};
		}

		ns.Save();

		var nf = Flow.CreateNested(os.Name, new Log());

		nf.WorkDirectory = os.Profile;

		os.Nexus = new Nexus(new NetBoot {Zone = Zone, Profile = os.Profile}, ns, vs, nf);
		os.Nexus.Vault.AuthenticationRequested = 
			(application, logo, net, user, key) => 
				new AuthenticationChoice {PublickKey = key ?? os.Nexus.Vault.Find(user).Public, Trust = Trust.AlwaysAllow};
		os.Nexus.Vault.AuthorizationRequested = (address, a, o) => true;

		if(os.Nexus.Vault.Wallets.Count == 0)
		{
			var w = os.Nexus.Vault.AddWallet(null, new Dictionary<SecretKey, string>(), G.Developer.Secrets.Password);
			foreach(var i in fs)
			{
				w.AddKey(i.Name, i.Key.Secret, i.Name);
			}
		}

		os.Stopped += s => {
			foreach(var i in os.Nodes)
				McvPeering.All.Remove(i.Peering);

			Oses.Remove(os);
			OsRemoved?.Invoke(os);
		};

		OsAdded?.Invoke(os);

		if(ip.Equals(Initials[0]))
		{
			F0 = os;
		}

		return os;
	}
}
