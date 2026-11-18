using System;
using System.IO;

using System.Threading;
using Uccs.Rdn;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests release local copy information query command `release local`. Basic smoke test;
/// Checking availability for non-existent release; Command parameters input testing.
/// </summary>
[TestType(TestType.CommandLine)]
class ReleaseLocal : RdnTest
{
	Actor a;
	const string releaseFile = "releaseFile.txt";
	LocalReleaseApe lra;

	public ReleaseLocal() : base("Release Local Command Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor();

		File.WriteAllText(releaseFile, "This is a content of a test release.");
		lra = CreateRelease(a.Os, releaseFile);
	} 

	public void Logic()
	{
		NetExec(a.Os, $"release local address={lra!.Address}");

		File.Delete(releaseFile);

		// Check availability after release file deletion
		// This test is expected to fail, reminding about the fact that ER is not determined yet! See issue for details.
		// @Issue: https://www.notion.so/Positive-release-local-availability-despite-release-file-absence-1cb4e2aecdb980f98014cfb5352de0f2?pvs=4
		ExpectException(() => NetExec(a.Os, $"release local address={lra!.Address}"));
		Thread.Sleep(1000);
		ExpectException(() => NetExec(a.Os, $"release local address={lra!.Address}"));

		// Checking local availability of non-existent release.
		ExpectException(() => NetExec(a.Os, $"release local address=Urrh:/60B2D0FA8AD69F63868E7DEBC83406389C83649841500A19B7C7481EE675FB9B"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) => ExpectException(() => NetExec(a.Os, s));

		string fmtcmd = $"release local address={{0}}";
		Prober.Probe(fmtcmd, f);
	}
}
