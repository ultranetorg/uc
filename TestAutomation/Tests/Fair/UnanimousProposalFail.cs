using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test is focused on a logic of unanimous voting fails. It is expected for unanimous proposal to 
/// fail after a single negative vote. Unanimous votes succeed only when all votes are positive, that's why even a single 
/// negative vote is enough to fail and cancel such voting.
/// </summary>
[TestType(TestType.Generic)]
class UnanimousProposalFail : FairTest
{
	List<Actor> authors;
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public UnanimousProposalFail() : base("Unanimous Proposal Fail Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(1);
		mods.Add(authors[0]);
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		// Total of 6 mods.

		ChangeStorePolicy(S.Id, authors, FairOperationClass.CategoryCreation, ApprovalRequirement.AllModerators);
	}

	public void Logic()
	{
		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation
			{
				Title = "New Category"
			}, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		Vote(S.Id, mods[..4], Role.Moderator, all: true);
		Vote(S.Id, [mods[4]], Role.Moderator, vote: (sbyte)SpecialChoice.Neither);

		Assert(S.Proposals.Length == 0);

		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation
			{
				Title = "New Category"
			}, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		Vote(S.Id, [mods[0]], Role.Moderator, vote: (sbyte)SpecialChoice.Neither);
		Assert(S.Proposals.Length == 0);
	}
}

