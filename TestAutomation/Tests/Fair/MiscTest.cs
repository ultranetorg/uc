using System;
using System.Collections.Generic;

using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// </summary>
[TestType(TestType.Generic)]
class MiscTest : FairTest
{
	Actor storeCreator;
	AutoId storeId;

	Store store => McvFind<Store>(storeId);

	public MiscTest() : base("Misc Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		storeCreator = CreateActor(topUp: true);
		storeId = CreateStore(storeCreator, "Store1", 1, topUp: true);
	}

	public override void Logic()
	{
		// Case where store creator and a first moderator attempts to change store policy.
		// By default store policy perpetual surveys can only be voted on by publishers of the store.
		// Moderator attempts, still, by supplying Publisher = null.
		NetExec(storeCreator.Os, [new SurveyVoting {
				Store = storeId,
				Publisher = null,
				Survey = store.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation).Id,
				Choice = store.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation).FindIndex(ApprovalRequirement.ModeratorsMajority)
			}], storeCreator.User.Name, expectFailure: true);

		Actor author = StoreAddNewAuthor(storeId, storeCreator);

		// Perpetual publisher voting executed by unauthorized 3rd party(store creator and a moderator in this case).
		// Failure expected.
		NetExec(storeCreator.Os, [new SurveyVoting {
				Store = storeId,
				Publisher = FindFairUser(author.User.Name).Authors[0],
				Survey = store.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation).Id,
				Choice = store.FindApprovalPolicyChangeSurvey(FairOperationClass.StoreInfoUpdation).FindIndex(ApprovalRequirement.ModeratorsMajority)
			}], storeCreator.User.Name, expectFailure: true);

	}

}

