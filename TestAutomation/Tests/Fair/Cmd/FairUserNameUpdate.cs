using Uccs.Net;
using Uccs.Nexus;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// Test user name update command `usern n`. 
/// </summary>
[TestType(TestType.CommandLine)]
class FairUserNameUpdate : RdnTest
{
	Actor a;

	public FairUserNameUpdate() : base("Fair User Name Update Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor();

		NetExec(a.Os, $"user e name={a.User.Name}");
	}

	public void Logic()
	{
		string newName = "new_name";
		NetExec(a.Os, $"user n newname={newName} by={a.User.Name}");

		// Trying to fetch user entity using old name, exception expected.
		ExpectException(() =>
			NetExec(a.Os, $"user e name={a.User.Name}"));

		// Fetching by new name.
		NetExec(a.Os, $"user e name={newName}");
		a.User.Name = newName;

		// Attempting to set name that is shorter than allowed minimum.
		// Exception expected.
		string less = GetStringOfLength(-1 + User.NameLengthMin);
		ExpectException(() =>
			NetExec(a.Os, $"user n newname={less} by={a.User.Name}"));

		// Attempting to set name that is longer than allowed minimum.
		// Exception expected.
		string more = GetStringOfLength(1 + User.NemaLengthMax);
		ExpectException(() =>
			NetExec(a.Os, $"user n newname={more} by={a.User.Name}"));
	}
}

