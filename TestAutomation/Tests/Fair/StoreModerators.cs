using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test simulates the e2e case of adding new moderators to the site. First, a site with 4 published authors is created. 
/// Then, moderation addition proposals are created and voted for. Each actor uses separate OS.
/// </summary>
[TestType(TestType.Generic)]
class StoreModerators: FairTest
{
	List<Actor> authors;
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public StoreModerators() : base("Site Moderators Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(4);
		mods.Add(authors[0]);
	}

	public override void Logic()
	{
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));


		Assert(S.Moderators.Length == 1 + 3, "Wrong final moderators count.");
	}

}

