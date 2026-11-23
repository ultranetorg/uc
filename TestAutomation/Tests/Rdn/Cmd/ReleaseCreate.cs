using System.IO;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Tests release creation command `release create`. 
/// - Basic smoke test.
/// - Attempting creation of a release from the file already in use by existing release, exception expected.
/// - Attempt to create release from nonexistent file, exception expected.
/// Command parameters fuzzy tested.
/// </summary>
[TestType(TestType.CommandLine)]
class ReleaseCreate : RdnTest
{
	Actor a;
	const string releaseFile = "releaseFile.txt";

	public ReleaseCreate() : base("Release Creation Test") {	}
	~ReleaseCreate()
	{
		File.Delete(releaseFile); // Cleanup.
	}

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor();
		File.WriteAllText(releaseFile, "This is a content of a test release.");
	}

	public override void Logic()
	{
		// Basic smoke case.
		CreateRelease(a.Os, releaseFile);

		// Attempting creation of a release from the file already in use by existing release, exception expected.
		ExpectException(() => CreateRelease(a.Os, releaseFile));

		// Attempt to create release from nonexistent file, exception expected.
		ExpectException(() => CreateRelease(a.Os, "nonexistentfile"));

		CmdFuzzTests();
	}

	void CmdFuzzTests()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"release create source={{0}}", f);
	}
}
