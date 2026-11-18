using O;
using Uccs.Net;
using Uccs.Rdn;

using CommonUtils;
using System;

namespace TestAutomation.Tests.Rdn;

[TestType(TestType.TestSet)]
class Dependencies : RdnTest
{
	public Dependencies() : base("Dependencies Test Set") { }

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public void Logic()
	{
		Actor a = CreateActor(topUp: true);
		string domain = GetUnusedLongDomainName();
		AcquireDomainWithDomainName(a, domain, years: 1);

		Ura dep0 = new(domain, "dep0");
		CreateResource(a,
				 dep0.ToString(),
				 $"{Meaning.Raw}{{hex=FFFFFF}}",
				 dependable: true);
		Ura dep1 = new(domain, "dep1");
		CreateResource(a,
				 dep1.ToString(),
				 $"{Meaning.Raw}{{hex=FFFFFF}}",
				 dependable: true);
		Ura dep2 = new(domain, "dep2");
		CreateResource(a,
				 dep2.ToString(),
				 $"{Meaning.Raw}{{hex=FFFFFF}}",
				 dependable: true);
		Ura resource = new(domain, "resource");
		CreateResource(a,
				 resource.ToString(),
				 $"{Meaning.Raw}{{hex=010101}}");

		// Link of type "dependency" pointing to "dependable" resource makes said 
		// resource "locked": can not be modified, deleted or change it's outgoing
		// "dependency" links.
		CreateLink(a, dep2, dep1, type: ResourceLinkType.Dependency);
		//
		//        dep0
		//                    
		//                    
		// res           dep1
		//                ^
		//                │
		//        dep2----|

		CreateLink(a, resource, dep0, type: ResourceLinkType.Dependency);
		//
		//  *---> dep0
		//  |         
		//  |         
		// res           dep1
		//                ^
		//                │
		//        dep2----*

		// Dependency 0 is locked (it is linked as a dependency from res) -
		// creating dependency link from it is expected to fail.
		CreateLink(a, dep0, dep2, type: ResourceLinkType.Dependency, ActionOnResult.ExpectFailure);
		//
		//  *---> dep0
		//  |      |  
		//  |      | 
		// res     X     dep1
		//         |      ^
		//         v      │
		//        dep2----*



		// Attempt to modify locked resources - failure expected.
		UpdateResource(a, dep0, $"{Meaning.Raw}{{hex=030303}}", 
			ActionOnResult.ExpectFailure);
		UpdateResource(a, dep1, $"{Meaning.Raw}{{hex=030303}}",
			ActionOnResult.ExpectFailure);
		// Can modify dep2 - not locked.
		UpdateResource(a, dep2, $"{Meaning.Raw}{{hex=030303}}");

		// Attempt to rename locked resource - failure expected.
		NetExec(a.Os, $"resource n address={dep0} newname=stinky_resource by={a.User.Name} aor=expectfailure");
		NetExec(a.Os, $"resource n address={dep1} newname=stinky_resource by={a.User.Name} aor=expectfailure");
		// Can modify dep2 - not locked.
		string newName = "new_resource_name";
		NetExec(a.Os, $"resource n address={dep2} newname={newName} by={a.User.Name}");
		dep2 = new(domain, newName);

		// Attempt to delete locked resources - failure expected.
		DeleteResource(a, dep0, ActionOnResult.ExpectFailure);
		DeleteResource(a, dep1, ActionOnResult.ExpectFailure);
		// Can delete dep2, not locked.
		DeleteResource(a, dep2);


		// TODO: Check that dependency persists even after domain expires.
		// But domains don't really expire yet... to discuss with @Maximion.
	}
}
