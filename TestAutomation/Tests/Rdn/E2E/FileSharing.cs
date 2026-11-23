using System;
using System.IO;
using Uccs.Rdn;
using Uccs.Net;

namespace TestAutomation.Tests.Rdn;


/// <summary>
/// This e2e test represents a use case of two users sharing a file using Ultranet.
/// User A creates a file, acquires a domain, locates resource containing a file there. Then 
/// user B downloads a file-containing resource and verifies its contents.
/// </summary>
[TestType(TestType.EndToEnd)]
class FileSharing : RdnTest
{
	string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
	string fileSendPath = "file.txt";
	string fileDownloadedPath = "fileDownloaded.txt";
	string fileContent = "This is a content of text release.";

	string domain = GetUnusedLongDomainName();

	public FileSharing() : base("File Sharing E2E Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		fileSendPath = Path.Combine(currentDir, fileSendPath);
		fileDownloadedPath = Path.Combine(currentDir, fileDownloadedPath);
	}

	public override void Logic()
	{
		// 1. User A creates a text file to share.
		File.WriteAllText(fileSendPath, fileContent);

		// 2. User A and user B create a wallet, user. Acquire utility tokens.
		Actor a = CreateActor(topUp: true);
		Actor b = CreateActor(topUp: true);

		// 3. User A creates a local personal domain to locate his file.
		CreateDomain(a, domain, years: 1);
		// 4. User A creates a release attaching a text file as a source.
		var lra = CreateRelease(a.Os, fileSendPath);
		// 5. User A creates a resource, linking it to a just created release.
		CreateResource(a, $"/{domain}/file", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}");
		// 6. User B downloads a resource, reads it.
		NetExec(b.Os, $"resource download /{domain}/file localpath={fileDownloadedPath}");
		

		if(!File.Exists(fileDownloadedPath))
			throw new Exception("Downloaded file is not found.");
		if(File.ReadAllText(fileDownloadedPath) != fileContent)
			throw new Exception("File content has been changed.");

		File.Delete(fileSendPath);
		File.Delete(fileDownloadedPath);
	}
}
