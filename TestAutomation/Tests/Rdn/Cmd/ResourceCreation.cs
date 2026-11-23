using System;
using Uccs.Rdn;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests resource creation command `resource create` by creating a 'raw' (of data type `Data`) resource 
/// and a counsil resource. Checks 'Status' output field; Command parameters are input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceCreation : RdnTest
{
	Actor a;
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	public ResourceCreation() : base("Resource Creation Command Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		AcquireDomainWithDomainName(a, domain, 1);
	}

	public override void Logic()
	{
		// No resource meaning/type hint. Exception expected.
		ExpectException(() =>
			CreateResource(a, $"/{domain}/no_identification", "12399999432049239423492830"));
		
		CreateResourceRaw(a, $"/{domain}/testResource", data);

		CreateResource(a, $"/{domain}/consil", $"{Meaning.Ampp_Council}{{analyzers={Net.Father0Signer} sefm=1 refm=1 rstfm=1}}");

		// Exactly maximum size for a resorce creation data.
		string hex = GetHexString(ResourceData.LengthMax);
		CreateResourceRaw(a, $"/{domain}/biggest", hex);

		// One more byte than maximum size for a resorce creation data. Exception expected.
		hex = GetHexString(ResourceData.LengthMax + 1);
		ExpectException(() => CreateResourceRaw(a, $"/{domain}/biggest_and_1", hex));

		// Hash is not real, but we don't care. According to @Maximion this is kind of "low level"
		// way of creating resources anyway, and a seed node will actually handle invalid hashes
		// (I forgot how exactly tho).
		Urn badUrn = GetZeroUrn();
		CreateResourceFile(a, $"/{domain}/bad_hash", badUrn);
		
		// Resource with provided RRRH already exists. Failure expected.
		CreateResourceFile(a, $"/{domain}/bad_hash", badUrn, ContentType.Image, aor: ActionOnResult.ExpectFailure);

		// Testing length limit. Exception expected.
		string huge = GetHugeLoremIpsum(1024 * 10, spaces: false);
		ExpectException(() => 
			CreateResourceRaw(a, $"/{domain}/{huge}", data));

		// Empty resource name in the URA. Exception expected.
		ExpectException(() => 
			CreateResourceRaw(a, $"/{domain}/", data));

		// Empty URA. Exception expected.
		ExpectException(() => 
			CreateResourceRaw(a, "", data));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		Action<string, ProbeType> f = (s, pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"resource create {{0}} data={Meaning.Raw}{{{{hex={data}}}}} by={a.User.Name}";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource create /{domain}/testResource2 data={{0}} by={a.User.Name}";
		Prober.Probe(fmtcmd, f);
		fmtcmd = $"resource create /{domain}/testResource2 data={Meaning.Raw}{{{{hex={data}}}}} by={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
