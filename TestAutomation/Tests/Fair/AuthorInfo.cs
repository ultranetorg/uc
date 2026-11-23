using Uccs.Net;
using Uccs.Fair;

using TestAutomation.ParamProber;
using Microsoft.AspNetCore.Components.Forms;
using static RocksDbSharp.ColumnFamilies;
using System.Text;
using System.Diagnostics;
namespace TestAutomation.Tests.Fair;

// From @a:
// title max 1
// description max 1
// ref many, max refs == 30
// new refs overwrite all current refs, so no need for deleteion/addition/organisation logic.
[TestType(TestType.Generic)]
class AuthorInfo : FairTest
{
	Actor a;
	AutoId authorId;
	const int maxRefs = 30;

	public AuthorInfo() : base("Author Info Change Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);

		authorId = CreateAuthor(a, "Author", years: 3, topUp: true);
	}

	public override void Logic()
	{
		// Setting a title.
		string newTitle = "Abobius The Third";
		NetExec(a.Os, $"author info id={authorId} title=\"{newTitle}\" by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Title == newTitle);

		// Setting a description.
		string newDesc = "I am Abobius author, the third. Fear me and my publications!";
		NetExec(a.Os, $"author info id={authorId} description=\"{newDesc}\" by={a.User.Name}");
		Assert(McvFind<Author>(authorId).Description == newDesc);

		// Setting a reference.
		string newRefText = "My stinky link";
		string newRefUri = "google.com.org.uk.abob";
		NetExec(a.Os, $"author info id={authorId} ref={{text=\"{newRefText}\" uri={newRefUri}}} by={a.User.Name}");
		Author au = McvFind<Author>(authorId);
		Assert(au.References[0].Text == newRefText && au.References[0].Uri == newRefUri);

		// Setting a reference again. Old reference should be overwritten.
		newRefText = "LinkyBadinky";
		newRefUri = "ultranet.com";
		NetExec(a.Os, $"author info id={authorId} ref={{text=\"{newRefText}\" uri={newRefUri}}} by={a.User.Name}");
		au = McvFind<Author>(authorId);
		Assert(au.References.Length == 1);
		Assert(au.References[0].Text == newRefText && au.References[0].Uri == newRefUri);

		string _ref = " ref={text=name uri=aboba.com}";
		StringBuilder builder = new();
		for (int i = 0; i < maxRefs; ++i) 
			builder.Append(_ref);

		// Maximum allowed amount of refs: 30.
		NetExec(a.Os, $"author info id={authorId} {builder} by={a.User.Name}");
		au = McvFind<Author>(authorId);
		Assert(au.References.Length == maxRefs);

		// 31 ref - one more than maximum, exception expected(Invalid).
		ExpectException(() => 
			NetExec(a.Os, $"author info id={authorId} {builder} {_ref} by={a.User.Name}"));

		// Multiple titles, bad, exception expected.
		ExpectException(() => 
			NetExec(a.Os, $"author info id={authorId} title=\"{newTitle}\" title=aboba_title by={a.User.Name}"));

		// Multiple descriptions, bad, exception expected.
		ExpectException(() => 
			NetExec(a.Os, $"author info id={authorId} description=\"{newDesc}\" description=aboba_title by={a.User.Name}"));
	}

}
