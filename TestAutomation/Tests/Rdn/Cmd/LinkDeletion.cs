using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This tests resource-to-resource link deletion with `link delete` command. Basic smoke testing;
/// Attempt to delete non-existent link; Command parameters input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class LinkDeletion : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public LinkDeletion() : base("Link Destroy Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		CreateResource(a, $"/{domain}/testResource1", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource2", $"{Meaning.Raw}{{hex={data}}}");
		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource2");
	}

	public void Logic()
	{
		// Default case.
		NetExec(a.Os, $"link delete from=/{domain}/testResource1 index=0 by={a.User.Name} aor=retryuntilconfirmed");

		// Nonexistent link index, failure expected.
		NetExec(a.Os, $"link delete from=/{domain}/testResource1 index=10 by={a.User.Name} aor=expectfailure");

		// Destroying link from nonexistent resource. Exception expected.
		ExpectException(() => NetExec(a.Os, $"link delete from=/{domain}/nonExistent index=0 by={a.User.Name} aor=retryuntilconfirmed"));


		ParamsTest();
	}

	void ParamsTest()
	{
		Action<string, ProbeType> f = (s, pt) => ExpectException(() => NetExec(a.Os, s));

		Prober.Probe($"link delete from={{0}} index=0 by={a.User.Name} aor=retryuntilconfirmed", f);
		Prober.Probe($"link delete from=/{domain}/testResource1 index={{0}} by={a.User.Name} aor=retryuntilconfirmed", f, 
			exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced]);
		Prober.Probe($"link delete from=/{domain}/testResource1 index=0 by={{0}} aor=retryuntilconfirmed", f);
		Prober.Probe($"link delete from=/{domain}/testResource1 index=0 by={a.User.Name} aor={{0}}", f);

	}
}
