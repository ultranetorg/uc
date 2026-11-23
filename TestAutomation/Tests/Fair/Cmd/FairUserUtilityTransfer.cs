using System;
using Uccs.Net;

using TestAutomation.ParamProber;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// Check that `utility transaction` command works as expected. Basic smoke testing;
/// Not enough funds test; Non-existent receiver entity id test; Non-existent sender entity id test;
/// Bad signer test; Each command line parameter is input tested.
/// </summary>
[TestType(TestType.CommandLine)]
class FairUserUtilityTransfer : FairTest
{
	Actor a1, a2;

	public FairUserUtilityTransfer() : base("Fair User Utility Transfer Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		a2 = CreateActor(topUp: false);
	}

	public override void Logic()
	{
		// Basic smoke testing: successful utility transaction.
		NetExec(a1.Os, $"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st=100by e=100 by={a1.User.Name}");

		//string e = "aor=" + ActionOnResult.ExpectFailure.ToString().ToLower();
		// Not enough funds - expected to fail.
		NetExec(a2.Os, $"utility t from=User/{a2.User.Id} to=User/{a1.User.Id} st=1000by e=100 by={a2.User.Name} aor=expectfailure");

		// Nonexistent receiver entity id. Expected to fail.
		NetExec(a1.Os, $"utility t from=User/{a1.User.Id} to=User/5016-0 st=100by e=100 by={a1.User.Name} aor=expectfailure");

		// Nonexistent sender entity id. Expected to fail.
		NetExec(a1.Os, $"utility t from=User/5016-0 to=User/{a2.User.Id} st=100by e=100 by={a1.User.Name} aor=expectfailure");

		// Bad signer. Expected to fail.
		ExpectException(() => NetExec(a1.Os, $"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st=100by e=100 by={a2.User.Name}"));

		ParamsTest();
	}

	void ParamsTest()
	{
		void a(string s, ProbeType pt) { ExpectException(() => NetExec(Oses[1], s)); }

		Prober.Probe($"utility t from={{0}} to=User/{a2.User.Id} st=100by e=100 by={a1.User.Name}", a);
		Prober.Probe($"utility t from=User/{a1.User.Id} to={{0}} st=100by e=100 by={a1.User.Name}", a);
		Prober.Probe($"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st={{0}} e=100 by={a1.User.Name}", a,
			exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced]);
		Prober.Probe($"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st=100by e={{0}} by={a1.User.Name}", a,
			exclude: [ProbeType.Numbers, ProbeType.NumbersSpaced]);
		Prober.Probe($"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st=100by e=100 by={{0}}", a);
		Prober.Probe($"utility t from=User/{a1.User.Id} to=User/{a2.User.Id} st=100by e=100 by={a1.User.Name} aor={{0}}", a);
	}
}
