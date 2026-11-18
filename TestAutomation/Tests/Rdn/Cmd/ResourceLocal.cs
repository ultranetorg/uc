using System;
using System.IO;

using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests query about locally available releases of a specified resource `resource local`.
/// Simple smoke test; Command parameters input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceLocal : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	LocalReleaseApe lra;
	string releaseFile = string.Empty;

	public ResourceLocal() : base("Resource Local Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		CreateDomain(a, domain, 1);
		CreateResource(a, $"/{domain}/testResource", $"{Meaning.Raw}{{hex={data}}}");

		releaseFile = CreateTestFile("file.txt");
		lra = CreateRelease(a.Os, releaseFile);
		CreateResource(a, $"/{domain}/testResourceFile", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}");
	}

	public void Logic()
	{
		// @Issue: https://www.notion.so/Local-availability-of-the-the-DataType-Data-resource-type-1cb4e2aecdb980d2a42fcfbeed794e08?pvs=4
		NetExec(a.Os, $"resource local address=/{domain}/testResource");
		AssertStatusConfirmedOutput();

		NetExec(a.Os, $"resource local address=/{domain}/testResourceFile");
		AssertStatusConfirmedOutput();

		File.Delete(releaseFile);

		// @Issue: https://www.notion.so/Positive-release-resource-local-availability-despite-release-file-absence-1cb4e2aecdb980f98014cfb5352de0f2?pvs=4
		NetExec(a.Os, $"resource local address=/{domain}/testResourceFile");

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"resource local address={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
