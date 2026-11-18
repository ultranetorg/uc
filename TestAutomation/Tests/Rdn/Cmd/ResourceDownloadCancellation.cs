using System;
using System.IO;
using Uccs.Rdn;
using Uccs.Net;
using System.Threading;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This tests command `resource dx` that supposed to cancel ongoing resource downloading.
/// This test downloads a 100mb file and executes the command while its happening. This is not a consistent 
/// way to test, discussion: 
/// https://www.notion.so/Test-resource-download-cancellation-with-resource-dx-1cb4e2aecdb980a2bf65d8ae72e4d1d2?pvs=4 ;
/// Command parameters are input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class ResourceDownloadCancellation : RdnTest
{
	Actor uploader, downloader;
	string currentDir = Directory.GetCurrentDirectory();
	string file = "100mb.txt";
	string fileDownloaded = "fileDownloaded.txt";

	string domain = GetUnusedLongDomainName();

	LocalReleaseApe lra;

	public ResourceDownloadCancellation() : base("Resource Download Cancellation Cmd Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		file = Path.Combine(currentDir, file);
		// Write 100mb of text
		File.WriteAllText(file, GetHugeLoremIpsum(102400, true, true));
		fileDownloaded = Path.Combine(currentDir, fileDownloaded);

		uploader = CreateActor(topUp: true);
		downloader = CreateActor(topUp: true);

		CreateDomain(uploader, domain, 1);
		lra = CreateRelease(uploader.Os, file);
		CreateResource(uploader, $"/{domain}/file", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}");
		NetExec(downloader.Os, $"resource download address=/{domain}/file localpath={fileDownloaded} nowait");
	}

	public void Logic()
	{
		Thread.Sleep(1500);
		NetExec(downloader.Os, $"resource dx address={lra.Address}");

		// Stoping download that is not happening. Exception expected.
		ExpectException(() => NetExec(downloader.Os, $"resource dx address={GetZeroUrn()}"));

		// According to @Maximion - presense of the file leftover is expected behaviour when canceling ongoing download: 
		// https://www.notion.so/Resource-download-cancellation-does-not-seem-to-work-file-still-gets-downloaded-1d64e2aecdb9800290cbd0daa65dcda9?pvs=4
		/*
		if (new FileInfo(fileDownloaded).Exists) {
			File.Delete(fileDownloaded);
			throw new Exception("File was downloaded despite calling download cancellation command.");
		}
		*/

		File.Delete(file);
		File.Delete(fileDownloaded);

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) => ExpectException(() => NetExec(downloader.Os, s));

		string fmtcmd = $"resource dx address={{0}}";
		Prober.Probe(fmtcmd, a);
	}
}
