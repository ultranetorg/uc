using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test checks the process of creating a nickname change proposal, and its approval. 
/// 3 authors 2/3 majority case; with new authors case; case with negative votes; Fail case. 
/// Nickname change dynamic verified after each case.
/// </summary>
[TestType(TestType.Generic)]
class StoreVoting : FairTest
{
	List<Actor> authors;
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	string newNick = "newgreatnickname";


	public StoreVoting() : base("Site Voting Test") {}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(3);
		mods.Add(authors[0]);
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));

		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreRenaming, ApprovalRequirement.PublishersMajority);
	}

	public override void Logic()
	{
		// Basic case: nickname change proposal -> 2/3 authors vote "Yes" -> assert nickname change.
		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new StoreRenaming
			{
				Name = newNick
			}, mods[0].User.Name, Role.Moderator);

		Vote(S.Id, authors, Role.Publisher);
		Assert(S.Name == newNick, "Nickname did not change.");

		// Same basic case but with new authors, 3/5 vote "Yes" -> assert nickname change.
		// TODO: What if authors added after proposal creation? Got to implement.
		authors.AddRange(SiteAddNewAuthors(2, S.Id, mods[0]));
		newNick = "newsicknickname";
		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new StoreRenaming
			{
				Name = newNick
			}, mods[0].User.Name, Role.Moderator);

		Vote(S.Id, authors, Role.Publisher);
		Assert(S.Name == newNick, "Name did not change.");

		// Case with negative votes. 1 "Yes" vote -> 2 "No" votes -> 2 "Yes" votes -> 3 vs 2 -> assert nickname change.
		newNick = "newhorriblenickname";
		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new StoreRenaming
			{
				Name = newNick
			}, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, [authors[0]], Role.Publisher, all: true);
		Vote(S.Id, [authors[1], authors[2]], Role.Publisher, all: true, (sbyte)SpecialChoice.Neither);
		Vote(S.Id, [authors[3], authors[4]], Role.Publisher, all: true);

		Assert(S.Name == newNick, "Name did not change.");

		// Fail case. 1 "Yes" vote -> 3 "No" votes -> assert proposal declined.
		string oldNick = newNick;
		newNick = "nicker";
		Propose(mods[0].Os, S.Id, mods[0].User.Id,
			new StoreRenaming
			{
				Name = newNick
			}, mods[0].User.Name, Role.Moderator);
		Vote(S.Id, [authors[0]], Role.Publisher, all:true);
		Vote(S.Id, [authors[2], authors[3]], Role.Publisher, all: true, (sbyte)SpecialChoice.Neither);

		Assert(S.Name != newNick && S.Name == oldNick , "Nickname changed despite proposal voting result - No.");

	}

}

