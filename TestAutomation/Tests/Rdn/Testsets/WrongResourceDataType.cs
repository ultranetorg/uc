using System.IO;
using Uccs.Rdn;
using Uccs.Net;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This set of tests checks cases when resource is being created with a wrong data type provided.
/// </summary>
[TestType(TestType.TestSet)]
class WrongResourceDataType : RdnTest
{
	string releaseDir;
	string releaseFile;

	public WrongResourceDataType() : base("Wrong Data Type Resource Creation Testset") {}

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public override void Abort()
	{
		base.Abort();
		// Clean up release files this test creates on test termination
		Directory.Delete(releaseDir, true);
		File.Delete(releaseFile);
	}

	public override void Logic()
	{
		Actor a1 = CreateActor();
		string domain = GetUnusedLongDomainName();
		CreateDomain(a1, domain, years: 1);

		// ExternalContent/File with hex.
		ExpectException(() => CreateResource(a1, $"/{domain}/hexInFile", 
			$"{Meaning.ExternalContent}/{ContentType.File}{{hex=5f5f5f5f5f}}"));

		// ExternalContent/Directory with hex.
		ExpectException(() => CreateResource(a1, $"/{domain}/hexInDir", 
			$"{Meaning.ExternalContent}/{ContentType.Directory}{{hex=5f5f5f5f5f}}"));

		releaseDir = CreateMinimalReleaseDirectory("TestDir");
		releaseFile = CreateTestFile("TestFile");

		LocalReleaseApe lraDir = CreateRelease(a1.Os, releaseDir);
		LocalReleaseApe lraFile = CreateRelease(a1.Os, releaseFile);

		// Why this is expected: https://www.notion.so/Using-incorrect-data-type-for-a-resource-sometimes-works-1cb4e2aecdb980b2b40ed28fe90428b5
		// ExternalContent/File with directory
		CreateResource(a1, $"/{domain}/dirInFile", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lraDir.Address}}}");
		// Data type "Directory" with file
		CreateResource(a1, $"/{domain}/fileInDir", $"{Meaning.ExternalContent}/{ContentType.Directory}{{address={lraFile.Address}}}");

		// Data type "Raw" with directory
		ExpectException(() => CreateResource(a1, $"/{domain}/dirInData", $"{Meaning.Raw}{{address={lraDir.Address}}}"));
		// Data type "Raw" with file
		ExpectException(() => CreateResource(a1, $"/{domain}/fileInData", $"{Meaning.Raw}{{address={lraFile.Address}}}"));

		// TODO: @Maximion What are these DataType enum values: Redirect; Redirect_ProductRealization; Redirect_Uri ?
	}
}
