using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;

using Uccs.Rdn;
using Uccs.Nexus;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// </summary>
[TestType(TestType.Generic)]
class PackagesTest : RdnTest
{
	Actor a1, a2;
	string domainName;

	public PackagesTest() : base("Packages Testset") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor();
		a2 = CreateActor();
	}

	public void Logic()
	{
		domainName = GetUnusedLongDomainName();
		AcquireDomainWithDomainName(a1, domainName, years: 1);

		string resourceName = "resource";
		string version = "0.0.1";
		string packageAddress = $"/{domainName}/{resourceName}/{version}";
		string sourcePath = Path.Join(Directory.GetCurrentDirectory(), "TestData", "0.0.1");
		PackageApe pa = CreatePackage(a1, packageAddress, sourcePath, publish: true);

		//LocalRelease r = a1.Os.GetLocalResourceByUrn(packageAddress);
		//CreateResource(a1, packageAddress, $"{Meaning.Package_Software_PackageManifest}/{ContentType.File}{{address={r.Address}}}");
		//CreateResource(a1, packageAddress, $"{ContentType.File}{{address={r.Address}}}");
		//CreateResource(a1, packageAddress, $"{Meaning.ExternalContent}/{ContentType.Software_Package}{{address={pa.Release.Address}}}");

		// Folder structure:
		// Packages/<domain>/<resource-name-version>/<package-files>
		// TODO krs: <resource-name-version> has a number between resource name and a version, what that means?
		// e.g.: `resource 47 0.0.1`.

		// Assert that before package deploy is executed - there is no downloaded packages exist.
		string packagesPath = Path.Join(a2.Os.GetProfileDirPath(this), "Packages");
		Assert(Directory.GetDirectories(packagesPath).Length == 0);
		
		// Not found before download: exception expected.
		ExpectException(() => NexusExec(a2.Os, $"package l address={packageAddress}"));

		NexusExec(a2.Os, $"package d address={packageAddress}");
		// Found after download.
		NexusExec(a2.Os, $"package l address={packageAddress}");

		NexusExec(a2.Os, $"package dp address={packageAddress}");

		// Now that package deploy was executed - check that package files exist.
		string pDir = Directory.GetDirectories(packagesPath).FirstOrDefault((dir) => 
			dir.Contains(domainName) && dir.Contains(resourceName) && dir.Contains(version));

		Assert(!string.IsNullOrEmpty(pDir));
		// Check that deployed package content is similar with uploaded package.
		Assert(AreDirectoryContentsSimilar(sourcePath, pDir, ignoreHidden: true));
		// Check that `package l` returns positive (No `NotFound` exception) now that package is deployed.
		NexusExec(a2.Os, $"package l address={packageAddress}");

	}
}
