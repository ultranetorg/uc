using Uccs.Net;

namespace TestAutomation.Tests.Rdn;

/// <summary>
/// This testset is a group of attempts to perform some kind of action 
/// on behalf of another user. Such actions should never go through and get executed.
/// There is an attempt to acquire domain using foreign signer account address.
/// Attempt to steal resources fron an account, signing with victim's account address 
/// and thief signing himself.
/// </summary>
[TestType(TestType.TestSet)]
class Impersonation : RdnTest
{
	Actor a, b;
	string domain = GetUnusedLongDomainName();

	public Impersonation() : base("Impersonation Testset") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		b = CreateActor(topUp: true);
	}

	public override void Logic()
	{
		// Attempt to acquire domain using foreign username.
		ExpectException(() => CreateDomain(a.Os, domain, b.User.Name, years: 1));

		// Attempt to steal resources from an account. Attempting to sign with victim's user.
		NetExec(a.Os, $"utility t from=User/{b.User.Id} to=User/{a.User.Id} st=10by e=10 by={a.User.Name} aor=expectfailure");

		// Attempt to steal resources from an account. Thief signs himself.
		NetExec(a.Os, $"utility t from=User/{b.User.Id} to=User/{a.User.Id} st=10by e=10 by={a.User.Name} aor=expectfailure");
	}

}
