using System;
using System.IO;

using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

[TestType(TestType.CommandLine)]
class AuthorAvatar : FairTest
{
	Actor a;
	AutoId authorId;

	string imagePath;

	int noMimeImageIdx, goodMimeImageIdx, notImageFileIdx;

	public AuthorAvatar() : base("Author Avatar Change Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		imagePath = GetTestImagePath();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 3);
		TopUpAuthor(authorId, 3 * GetFileSpacetimeRequirement(imagePath, 3 * 365)); // For 3 test images.
		TopUpAuthor(authorId); // And then some for a text file.
		// Image with no MIME.
		noMimeImageIdx = 0;
		CreateAuthorFile(a, authorId, imagePath);

		// Image with correct MIME (JPG).
		goodMimeImageIdx = 1;
		CreateAuthorFile(a, authorId, imagePath, FairMime.ImageJpg);

		// Not an image, no MIME.
		notImageFileIdx = 2;
		string filePath = CreateTestFile("text_file.txt");
		CreateAuthorFile(a, authorId, filePath);
	}

	public override void Logic()
	{
		// Image with no MIME, failure expected.
		AutoId fileId = McvFind<Author>(authorId).Files[noMimeImageIdx];
		SetAuthorAvatar(a, authorId, fileId, ActionOnResult.ExpectFailure);
		
		// Correct MIME.
		fileId = McvFind<Author>(authorId).Files[goodMimeImageIdx];
		SetAuthorAvatar(a, authorId, fileId);
		
		// Not an image, failure expected.
		fileId = McvFind<Author>(authorId).Files[notImageFileIdx];
		SetAuthorAvatar(a, authorId, fileId, ActionOnResult.ExpectFailure);

		ParamsTest();
	}

	void ParamsTest()
	{
		void f(string s, ProbeType pt) { ExpectException(() => NetExec(a.Os, s)); }

		Prober.Probe($"author avatar {{0}} file={new AutoId()} by={a.User.Name}", f);
		Prober.Probe($"author avatar {authorId} file={{0}} by={a.User.Name}", f);
		Prober.Probe($"author avatar {authorId} file={new AutoId()} by={{0}}", f);
	}
}
