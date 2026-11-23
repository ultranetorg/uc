using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test simulates the cases of new site categories creation. First, a site with 4 published authors is created. Few moderators are added. 
/// Then, new category creation dispute is created, voted for, results validated. Another category, same process. Sub-category creation, validated. Sub-sub-category creation, validated. 
/// Each actor uses separate OS.
/// </summary>
[TestType(TestType.Generic)]
class StoreCategoryCreation: FairTest
{
	List<Actor> authors = [];
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public StoreCategoryCreation() : base("Site Category Creations Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(4);
		mods.Add(authors[0]);

		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));

		ChangeStorePolicy(S.Id, authors, FairOperationClass.CategoryCreation, ApprovalRequirement.AllModerators);
	}

	public override void Logic()
	{
		Assert(S.Categories.Length == 0);

		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation { Title = "Social" }, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Categories.Length == 1, "New category expected to be added.");

		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation { Title = "Games" }, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Assert(S.Categories.Length == 2, "New category expected to be added.");

		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation { Parent = S.Categories[^1], Title = "Games" }, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		Category gamesCategory = McvFind<Category>(S.Categories[^1]);

		Assert(S.Categories.Length == 2, "No new root categories is expected to be added.");
		Assert(gamesCategory.Categories.Length == 1, "New sub-category expected to be added.");

		Category shootersSubCategory = McvFind<Category>(gamesCategory.Categories[^1]);

		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new CategoryCreation { Store = S, Parent = shootersSubCategory.Id, Title = "Doom" }, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		Vote(S.Id, mods, Role.Moderator, all: true);

		gamesCategory = McvFind<Category>(S.Categories[^1]);
		shootersSubCategory = McvFind<Category>(gamesCategory.Categories[^1]);

		Assert(S.Categories.Length == 2, "No new root categories is expected to be added.");
		Assert(gamesCategory.Categories.Length == 1, "New sub-category expected to be added.");
		Assert(shootersSubCategory.Categories.Length == 1, "New sub-category expected to be added.");
	}

}

