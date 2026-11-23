using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests command `link li` - list all links that point to this resource. Basic smoke testing;
/// Links count verification; Listing for a non-existent resource; Command parameters input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class LinkListInbounds : RdnTest
{
	Actor a;
	uint outOffset = 3;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public LinkListInbounds() : base("Link List Inbounds Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);

		CreateResource(a, $"/{domain}/testResource1", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource2", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource3", $"{Meaning.Raw}{{hex={data}}}");
		CreateResource(a, $"/{domain}/testResource4", $"{Meaning.Raw}{{hex={data}}}");

		CreateLink(a, $"/{domain}/testResource1", $"/{domain}/testResource2");
	}

	public override void Logic()
	{
		Flow.Log.Messages.Clear();
		NetExec(a.Os, $"link li address=/{domain}/testResource2");
		uint expected = 1;
		uint count = (uint)Flow.Log.Messages.Count - outOffset;
		Assert(expected == count, $"Wrong links count. Expected {expected}, listed {count}");

		CreateLink(a, $"/{domain}/testResource3", $"/{domain}/testResource2");
		CreateLink(a, $"/{domain}/testResource4", $"/{domain}/testResource2");
		Flow.Log.Messages.Clear();
		NetExec(Oses[1], $"link li address=/{domain}/testResource2");
		expected = 3;
		count = (uint)Flow.Log.Messages.Count - outOffset;
		Assert(expected == count, $"Wrong links count. Expected {expected}, listed {count}");

		// List inbound links for non-existent resource. Expected to fail, of course.
		ExpectException(() => NetExec(a.Os, $"link li address=/{domain}/nonExistent"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"link li address={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
