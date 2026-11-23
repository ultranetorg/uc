using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests command to destroy a resource `resource destroy`. Basic smoke test, attempt to destroy non-existant just destroyed resource;
/// Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceDestroy : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();
	Ura resUra;

	public ResourceDestroy() : base("Resource Destroy Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		resUra = new(domain, "testResource");

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		CreateResource(a, resUra.ToString(), $"{Meaning.Raw}{{hex={data}}}");
	}

	public override void Logic()
	{
		NetExec(a.Os, $"resource x address={resUra} by={a.User.Name}");

		// Attempt to get info about destroyed resource. Exception expected.
		ExpectException(() =>NetExec(a.Os, $"resource e address={resUra}"));

		Assert(FindResource(resUra) == null, "Destroyed resource should not be found.");

		// Attempt to destroy non-existant just destroyed resource, expected to fail.
		ExpectException(() => NetExec(a.Os, $"resource x address={resUra} by={a.User.Name} aor=expectfailure"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"resource x address={{0}} by={a.User.Name}";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource x address={resUra} by={{0}}";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource x address={resUra} by={a.User.Name} aor={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
