using System.IO;
using Uccs.Rdn;
using Uccs.Net;

using JunitReporter;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This e2e test represents a use case when attempt to publish a analysis result is being made by a user
/// that is not a part of counsil.
/// </summary>
[TestType(TestType.EndToEnd)]
class OutsiderAnalysisPublication : RdnTest
{
	readonly string domain1 = GetUnusedLongDomainName();
	readonly string domain2 = GetUnusedLongDomainName();

	public OutsiderAnalysisPublication() : base("Analysis Publication By Non-Affiliated E2E Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public override void Logic()
	{
		// Creator of the analysis counsil, one of the analyzers in this counsil.
		Actor a1 = CreateActor(topUp: true);

		// This is user A - creator of the resource and requestor of the analysis.
		Actor a2 = CreateActor(topUp: true);

		// This is user that will attempt to create a analysis report despite not being a part of counsil.
		Actor a3 = CreateActor(topUp: true);

		// Domain where counsil lives.
		CreateDomain(a1, domain1, 1);
		// Domain for resource to analise and analysis resource.
		CreateDomain(a2, domain2, 1);

		// Counsil creation
		NetExec(a1.Os, $"resource c /{domain1}/consil data={Meaning.Ampp_Council}{{analyzers={Net.Father0Signer},{a1.User.Key} sefm=1 refm=1 rstfm=1}} by={a1.User.Name}");

		// Resource-to-analyze creation - single file.
		string path = CreateTestFile("releaseFile.txt");
		LocalReleaseApe lra = CreateRelease(a2.Os, path);
		CreateResource(a2, $"/{domain2}/resource", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}");

		// Creation of analysis "request".
		NetExec(a2.Os, $"resource c /{domain2}/request data={Meaning.Ampp_Analysis}{{release={lra.Address} ereward={(1 + 1) * 3} streward={1 * 3} consil=/{domain1}/consil}} by={a2.User.Name}");
		CreateLink(a2, $"/{domain2}/request", $"/{domain2}/resource");

		Reporter.Message(CommonUtils.StandardOutputStreamType.Out, "Attempting.");
		// Non-affiliated to counsil account trying to sneak in a result. Expected to return fail.
		NetExec(a3.Os, 
			[new AnalysisResultUpdation { Analysis = FindResource(new Ura(domain2, "request")).Id, Result = AnalysisResult.Negative }], 
			a3.User.Name, expectFailure: true);

		File.Delete(path);

	}
}
