using System.IO;

using Uccs.Rdn;
using Uccs.Net;
using System.Diagnostics;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests locally available resource search `resource localsearch`.
/// Simple smoke test, checking that correct amount of results returned, checking 
/// that they are valid and contain the search substring. Checking that resource is 
/// not returned after associated files are deleted.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceLocalSearch : RdnTest
{
	string data = "48656C6C6F2C20576F726C6421";
	string domain = GetUnusedLongDomainName();

	LocalReleaseApe lra;
	Actor a;
	string releaseFile = string.Empty;
	uint outputOffset = 3;

	public ResourceLocalSearch() : base("Resource Local Search Command Test") {}

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
		Flow.Log.Messages.Clear();
		// @Issue: https://www.notion.so/Local-availability-of-the-the-DataType-Data-resource-type-1cb4e2aecdb980d2a42fcfbeed794e08?pvs=4
		string search = "Resource";
		NetExec(a.Os, $"resource localsearch {search} aor=retryuntilconfirmed");
		/* Expected result
		resource localsearch Resource aor=retryuntilconfirmed
		   Address                   Releases  Type        Data                                                              Length
		   ────────────────────────  ────────  ──────────  ────────────────────────────────────────────────────────────────  ──────
		   _domain/testResource      1         0, Unknown  48656C6C6F2C20576F726C6421                                        13
		   _domain/testResourceFile  1         1, Unknown  557272683A2F303433323541363144....304435334441413634344335463031  70
		*/
		uint expected = 2;
		uint results = (uint)Flow.Log.Messages.Count - outputOffset; // discarding the input command line itself, output header and separator lines
		Assert(results == expected, $"Search result count incorrect. Expected {expected}, got {results}.");
		for(int i = (int)outputOffset; i < Flow.Log.Messages.Count; ++i)
		{
			string s = string.Join("", Flow.Log.Messages.ToArray()[i].Text);
			Assert(s.Contains(search), "Search result entry does not contain search substring.");
		}

		Flow.Log.Messages.Clear();
		search = "File";
		NetExec(a.Os, $"resource localsearch {search} aor=retryuntilconfirmed");
		expected = 1;
		results = (uint)Flow.Log.Messages.Count - outputOffset; // discarding the input command line itself, output header and separator lines
		Assert(results == expected, $"Search result count incorrect. Expected {expected}, got {results}.");
		for(int i = (int)outputOffset; i < Flow.Log.Messages.Count; ++i)
		{
			string s = string.Join("", Flow.Log.Messages.ToArray()[i].Text);
			Assert(s.Contains(search), "Search result entry does not contain search substring.");
		}

		File.Delete(releaseFile);

		// @Issue: https://www.notion.so/Positive-release-resource-local-availability-despite-release-file-absence-1cb4e2aecdb980f98014cfb5352de0f2?pvs=4
		// Attempting to search resource, associated file of which was deleted.
		Flow.Log.Messages.Clear();
		NetExec(a.Os, $"resource localsearch File");
		//expected = 0;
		expected = 1;
		results = (uint)Flow.Log.Messages.Count - outputOffset; // discarding the input command line itself, output header and separator lines
		Assert(results == expected, $"Search result count incorrect. Expected {expected}, got {results}.");

		// No command parameter input testing because of this minor issue: 
		// https://www.notion.so/Resource-local-search-accepts-string-with-all-kinds-of-special-characters-1f14e2aecdb98098b290f3450fd5b23b?pvs=4
	}

}

