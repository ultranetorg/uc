using System;
using System.Collections.Generic;

using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test simulates the case of changing policies of the site. First, a site with 4 published authors is created. 
/// Then, policy change dispute is created and voted for. Voting results are validated. Change to each policy verified. Each actor uses separate UOS.
/// </summary>
[TestType(TestType.Generic)]
class StorePolicyChange : FairTest
{
	List<Actor> authors = [];
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public StorePolicyChange() : base("Site Policy Change Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(4);
		mods.Add(authors[0]);

		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
	}

	public void Logic()
	{
		// AnyModerator -> ModeratorsMajority
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, ApprovalRequirement.ModeratorsMajority);
		Survey ps = S.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation);
		Assert(ps.FindIndex(ApprovalRequirement.ModeratorsMajority) == ps.LastWin, "New policy is not applied.");

		// ModeratorsMajority -> AllModerators
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, ApprovalRequirement.AllModerators);
		ps = S.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation);
		Assert(ps.FindIndex(ApprovalRequirement.AllModerators) == ps.LastWin, "New policy is not applied.");

		// AllModerators -> PublishersMajority
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, ApprovalRequirement.PublishersMajority);
		ps = S.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation);
		Assert(ps.FindIndex(ApprovalRequirement.PublishersMajority) == ps.LastWin, "New policy is not applied.");

		// PublishersMajority -> AnyModerator
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, ApprovalRequirement.AnyModerator);
		ps = S.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation);
		Assert(ps.FindIndex(ApprovalRequirement.AnyModerator) == ps.LastWin, "New policy is not applied.");

	}

}

