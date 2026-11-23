using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests command `resource update`. Basic smoke test of updating an existing `Data` resource with new data.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceUpdate : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string newData = "55706461746564202044617461";

	string domain = GetUnusedLongDomainName();

	public ResourceUpdate() : base("Resource Update Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		CreateResource(a, $"/{domain}/testResource", $"{Meaning.Raw}{{hex={data}}}");
	}

	public override void Logic()
	{
		NetExec(a.Os, $"resource update /{domain}/testResource data={Meaning.Raw}{{hex={newData}}} by={a.User.Name}");

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		newData = "F2C20576F7";
		string fmtcmd = $"resource update {{0}} data={Meaning.Raw}{{{{hex={newData}}}}} by={a.User.Name} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource update /{domain}/testResource data={{0}} by={a.User.Name} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource update /{domain}/testResource data={Meaning.Raw}{{{{hex={newData}}}}} by={{0}} aor=retryuntilconfirmed";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource update /{domain}/testResource data={Meaning.Raw}{{{{hex={newData}}}}} by={a.User.Name} aor={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
