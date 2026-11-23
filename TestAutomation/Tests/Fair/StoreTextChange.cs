using System;
using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test simulates the cases of changing site text properties: description, slogan and title. Multiple changes, including a change under a different policy. 
/// Description changes verified. All actors represented by unique OS-es.
/// </summary>
[TestType(TestType.Generic)]
class StoreTextChange : FairTest
{
	List<Actor> authors = [];
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public StoreTextChange() : base("Site Text Change Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(4);
		mods.Add(authors[0]);
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));

		// Switch to a different policy, for additional complexity.
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, 
			ApprovalRequirement.AllModerators);
	}

	public override void Logic()
	{
		string newDesc = "New Description.";
		Propose(mods[0].Os, S.Id, FindFairUser(mods[0].User.Name).Id,
			new Uccs.Fair.StoreInfoUpdation
			{
				Description = newDesc
			}, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Description == newDesc, "Description did not change.");

		string newSlogan = "New Sloga.";
		Propose(mods[0].Os, S.Id, FindFairUser(mods[0].User.Name).Id,
			new Uccs.Fair.StoreInfoUpdation
			{
				Slogan = newSlogan
			}, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Slogan == newSlogan, "Slogan did not change.");

		string newTitle = "New Title.";
		Propose(mods[0].Os, S.Id, FindFairUser(mods[0].User.Name).Id,
			new Uccs.Fair.StoreInfoUpdation
			{
				Title = newTitle
			}, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Title == newTitle, "Title did not change.");

		newDesc = "Even newer description.";
		Propose(mods[0].Os, S.Id, FindFairUser(mods[0].User.Name).Id,
			new Uccs.Fair.StoreInfoUpdation
			{
				Description = newDesc,
			}, mods[0].User.Name, Role.Moderator);

		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Description == newDesc, "Description did not change.");

		// Back to original policy.
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, 
			ApprovalRequirement.AnyModerator);

		newDesc = "Juciest, the most fresh and new description.";
		Propose(mods[0].Os, S.Id, FindFairUser(mods[0].User.Name).Id,
			new Uccs.Fair.StoreInfoUpdation
			{
				Description = newDesc,
			}, mods[0].User.Name, Role.Moderator);

		Assert(S.Description == newDesc, "Description did not change.");
	}
}

