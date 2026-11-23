using System;

using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests command to list all outgoing links or a resource - `link lo`. Basics smoke test;
/// Links count verification; Link listing for non-existent resource; Command parameters input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class LinkListOutbounds : RdnTest
{
	Actor a;
	uint outOffset = 3;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public LinkListOutbounds() : base("Link List Outbounds Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);

		CreateResource(a, $"/{domain}/testResource1", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource2", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource3", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource4", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource5", $"{Meaning.Raw}{{hex={data}}}");
		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource2");
	}

	public override void Logic()
	{
		Flow.Log.Messages.Clear();
		NetExec(a.Os, $"link lo address=/{domain}/testResource1");
		uint expected = 1;
		uint count = (uint)Flow.Log.Messages.Count - outOffset;
		Assert(expected == count, $"Wrong links count. Expected {expected}, listed {count}");

		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource3");
		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource4");
		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource5");

		Flow.Log.Messages.Clear();
		NetExec(a.Os, $"link lo address=/{domain}/testResource1");
		expected = 4;
		count = (uint)Flow.Log.Messages.Count - outOffset;
		Assert(expected == count, $"Wrong links count. Expected {expected}, listed {count}");

		// Listing links for non-existent resource. Expected to fail, of course.
		ExpectException(() => NetExec(a.Os, $"link lo address=/{domain}/nonExistent"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"link lo address={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
