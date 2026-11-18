using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using O;
using Uccs;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Rdn.CLI;

namespace TestAutomation.Tests.Rdn;

[TestNetwork(TestNetwork.Rdn)]
abstract class RdnTest : TestBase
{
	protected override McvNode GetNode(Os os) => os.FindNode<RdnNode>();
	protected override McvCli GetCli(Os os) =>
		os.Clis.TryGetValue(os.FindNode<RdnNode>(), out var a) ? a :
			(os.Clis[os.FindNode<RdnNode>()] = new RdnCli(os.Nexus.Settings, os.FindNode<RdnNode>().Settings, new RdnApiClient(os.FindNode<RdnNode>().Settings.Api.LocalNodeAddress(Net)))
			{
				Application = os.Name
			});
	protected override McvNodeSettings CreateSettings(Os os) => new RdnNodeSettings(Path.Join(os.Profile, "rdn"));


	public RdnTest(string testName) : base(testName)
	{
		base.Init();

		Net = Uccs.Rdn.Rdn.TA;
		Group = new OsGroup(Path.Combine(G.Developer.Tmp, this.GetType().Name), Net.Initials);

		// OL.CreateHive(4, [Net]);
	}

	public override McvNode StartNode(Os os, McvNodeSettings settings)
	{
		os.Nexus.RunRdn(settings as RdnNodeSettings, Clock);

		return os.Nexus.RdnNode;
	}

	protected override void Configure(Os os, Xon args, McvNodeSettings settings)
	{
		var r = settings as RdnNodeSettings;

		r.DataPath = Path.Join(G.Developer.Root, "UC", nameof(Rdn), "Data");

		if(args.Has("seed"))
		{
			r.Seed = new();
			r.Seed.RefreshInterval = 1;
		}
	}

	public override List<Os> CreateMinimalNetwork()
	{
		List<Os> oses = [];

		oses.Add(Group.CreateOs("U0", "f=2 nn", Net.Father0EP.IP));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U1", "f=2 nn", Group.Initials[1]));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U2", "f=2 nn", Group.Initials[2]));
		CreateNode(oses[^1], "mcv chain g=2 s run");

		SyncAllPeering();

		return oses;
	}

	public override Os CreateOsNodePair()
	{
		Os os = Group.CreateOs($"U{Oses.Count}", "nn", GetUnusedIp());
		CreateNode(os, "seed run");
		return os;
	}

	public void UpdateResource(Actor a, Ura resource, string newData, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"resource u address={resource} data={newData} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	public void DeleteResource(Actor a, Ura resource, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"resource x address={resource} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	public void DeleteLink(Actor a, Ura linkSource, int linkIndex, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"link x from={linkSource} index={linkIndex} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	protected Resource FindResource(Ura resource)
	{
		RdnMcv mcv = (RdnMcv)GetNode(Oses[0]).Mcv;

		lock(mcv.Lock)
			return mcv.Resources.Latest(resource);
	}

	protected Domain FindDomain(string domain)
	{
		RdnMcv mcv = (RdnMcv)GetNode(Oses[0]).Mcv;

		lock(mcv.Lock)
			return mcv.Domains.Latest(domain);
	}

	public Urn GetZeroUrn()
	{
		//return new Rrrh { Hash = Net.Cryptography.ZeroHash };
		//return new Blake3Urn { Hash = Net.Cryptography.ZeroHash };
		return Urn.Blake3(Net.Cryptography.ZeroHash);
	}
}
