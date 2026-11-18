using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests resource information query command `resource entity`. Simple smoke test; Command parameter input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceEntity : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public ResourceEntity() : base("Resource Entity Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		CreateResource(a, $"/{domain}/testResource", $"{Meaning.Raw}{{hex={data}}}");
	}

	public void Logic()
	{
		NetExec(a.Os, $"resource entity address=/{domain}/testResource");

		// TODO krs
		// This was an old version of this test. With recent updates this 'NetExec()' method
		// does not return a Resource object anymore. Need to remake this, probably check
		// output text directly.
		/*
		Resource r = (Resource)NetExec(a.Os, $"resource entity address=/{domain}/testResource");

		if(!r.Data.Hex.EndsWith(data))
			throw new Exception($"Retrieved resource data does not match with the expected data. " +
					$"Retrieved: {r.Data.Hex}. Expected: {data}");
		*/

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"resource entity address={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
