using System;
using System.IO;
using Uccs.Rdn;
using Uccs.Net;

using CommonUtils.Ext;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This e2e test represents a use case when user A shares a folder with user B using Ultranet. User A 
/// acquires a domain and locates a directory-resource there. User B then, downloads a resource and reads folder 
/// contents, verifying them.
/// </summary>
[TestType(TestType.EndToEnd)]
class DirectorySharing : RdnTest
{
	string currentDir = Directory.GetCurrentDirectory();
	string dir = "dirShared";
	string file1 = "file1.txt";
	string file2 = "file2.txt";
	string dirDown = "dirDown";
	string fileContent1 = "This is a content of text file 1.";
	string fileContent2 = "This is a content of text file 2.";

	string domain = GetUnusedLongDomainName();

	public DirectorySharing() : base("Directory Sharing E2E Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		dir = Path.Combine(currentDir, dir);
		dirDown = Path.Combine(currentDir, dirDown);
	}

	public override void Logic()
	{
		// 1. Users A creates a folder with files to share.
		Directory.CreateDirectory(dir);
		File.WriteAllText(Path.Combine(dir, file1), fileContent1);
		File.WriteAllText(Path.Combine(dir, file2), fileContent2);

		// 2. Users A and B each create a wallet, user, top it up.
		Actor a = CreateActor(topUp: true);
		Actor b = CreateActor(topUp: true);

		// 3. User A creates a local personal domain to locate his file.
		CreateDomain(a, domain, years: 1);
		// 4. User A creates a release attaching a folder as source.
		var lra = CreateRelease(a.Os, dir);
		// 5. User A creates a resource, linking it to a just created release.
		CreateResource(a, $"/{domain}/share", $"{Meaning.ExternalContent}/{ContentType.Directory}{{address={lra.Address}}}");
		// 6. User B downloads a resource, reads it.
		NetExec(b.Os, $"resource download /{domain}/share localpath={dirDown}");

		if(!Directory.Exists(dirDown))
			throw new Exception("Downloaded directory is not found.");
		if(!File.Exists(Path.Combine(dirDown, file1)) || !File.Exists(Path.Combine(dirDown, file2)))
			throw new Exception("Downloaded files are not found.");
		if(File.ReadAllText(Path.Combine(dirDown, file1)) != fileContent1 ||
			File.ReadAllText(Path.Combine(dirDown, file2)) != fileContent2)
			throw new Exception("Files content has been changed.");

		new DirectoryInfo(dir).Empty();
		new DirectoryInfo(dirDown).Empty();
		Directory.Delete(dir);
		Directory.Delete(dirDown);
	}
}
