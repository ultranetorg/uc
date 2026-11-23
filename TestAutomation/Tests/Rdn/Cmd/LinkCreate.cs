using System;
using System.Linq;
using System.Threading;

using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This tests a resource-to-resource link creation with 'link create` command. Basic smoke testing;
/// Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class LinkCreate : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public LinkCreate() : base("Link Creation Test")	{}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		CreateDomain(a, domain, 1);
		CreateResource(a, $"/{domain}/testResource1", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource2", $"{Meaning.Raw}{{hex={data}}}");
	}

	public override void Logic()
	{
		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource2");

		// TODO: Test link creation from/to non-existent resources.

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"link create from={{0}} to=/{domain}/testResource2 by={a.User.Name} aor=retryuntilconfirmed", f);
		Prober.Probe($"link create from=/{domain}/testResource1 to={{0}} by={a.User.Name} aor=retryuntilconfirmed", f);
		Prober.Probe($"link create from=/{domain}/testResource1 to=/{domain}/testResource2 by={{0}} aor=retryuntilconfirmed", f);
		Prober.Probe($"link create from=/{domain}/testResource1 to=/{domain}/testResource2 by={a.User.Name} aor={{0}}", f);
	}
}
