using System;

using Uccs.Net;
using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;


/// <summary>
/// This tests migration of a web domain to the Ultranet with a `domain migrate` command.
/// Migration represents a process of acquiring a domain name on the basis of owning a web domain in the traditional web.
/// User has to locate his Ultranet account address in the DNS records of a web domain to be able to migrate.
/// - Basic smoke testing; 
/// - Non-existing domain test; 
/// Command parameters are input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class DomainNameMigrate : RdnTest
{
	Actor a;
	string domain = "ultranet";

	public DomainNameMigrate() : base("Domain Migrate Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
	}

	public override void Logic()
	{
		NetExec(a.Os, $"domainname m name={domain} wtld=org by={a.User.Name}");

		// Non-affiliated domain. Exception expected.
		ExpectException(() =>
			NetExec(a.Os, $"domainname m name=boobainehehehh wtld=org by={a.User.Name}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) => ExpectException(() => NetExec(Oses[0], s));

		Prober.Probe($"domainname m name={{0}} wtld=org by={Net.Father0Name} aor=retryuntilconfirmed", a);
		Prober.Probe($"domainname m name={domain} wtld={{0}} by={Net.Father0Name} aor=retryuntilconfirmed", a);
		Prober.Probe($"domainname m name={domain} wtld=org by={{0}} aor=retryuntilconfirmed", a);
	}
}
