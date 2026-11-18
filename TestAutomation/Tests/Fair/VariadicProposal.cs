using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test checks the process of creating a proposal with multiple options, and different cases of voting for it. 
/// Results are verified.
/// </summary>
[TestType(TestType.Generic)]
class VariadicProposal : FairTest
{
	List<Actor> authors;
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	string badNickMsg = "Voted nickname is not applied.";
	string proposalExistsMsg = "Proposal was not deleted.";

	public VariadicProposal() : base("Variadic Proposal Test")
	{
	}

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(4);
		mods.Add(authors[0]);
		authors.AddRange(SiteAddNewAuthors(3, S.Id, mods[0], Oses[1]));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));

		// 7 authors across 4 UOSes.
		// 4 moderators across 4 UOSes.
		// 10 actors across 7 UOSes in total.
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreRenaming, ApprovalRequirement.PublishersMajority);
	}

	public void Logic()
	{
		// Proposal for a new site nickname with multiple options(authors majority needed).
		// Authors vote for a particular option until sufficient amount of votes reached.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, authors, Role.Publisher, vote: 1);

		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick2", badNickMsg);

		// Same as previous case but with a diverse votes.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, [authors[0], authors[1]], Role.Publisher, all: true, vote: 2); // 2/7
		Vote(S.Id, [authors[2], authors[3]], Role.Publisher, all: true, vote: 0);  // 4/7
		Vote(S.Id, [authors[4]], Role.Publisher, vote: 1);  // 5/7
		Vote(S.Id, [authors[5], authors[6]], Role.Publisher, all: true, vote: 2);  // 7/7
													 // 2/1/4
		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick3", badNickMsg);

		// Most voters are indifferent - "Abstained" votes. And then one defining the outcome.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, authors[..3], Role.Publisher, all: true, vote: (sbyte)SpecialChoice.Any);
		Vote(S.Id, [authors[3]], Role.Publisher, vote: 1);

		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick2", badNickMsg);

		// All "abstained" votes. First option expected to be selected.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, authors, Role.Publisher, vote: (sbyte)SpecialChoice.Any);

		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick1", badNickMsg);

		// All "neither" votes. No changes should happen.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, authors, Role.Publisher, vote: (sbyte)SpecialChoice.Neither);

		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick1", badNickMsg);

		// All "ban" votes. No changes should happen. Creator should be banned.
		ProposeOpts(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[0],
			[new(new StoreRenaming {Name = "nick1"}),
			new(new StoreRenaming {Name = "nick2"}),
			new(new StoreRenaming {Name = "nick3"})], authors[0].User.Name, Uccs.Fair.Role.Publisher);
		Vote(S.Id, authors, Role.Publisher, vote: (sbyte)SpecialChoice.Ban);

		AssertNoProposals(S, proposalExistsMsg);
		Assert(S.Name == "nick1", badNickMsg);
		// TODO: This assert is not very good. Need to fix and check if BannedTill value is == Time.Now + 30 days exactly. See ProposalVoting.cs:178.
		Assert(S.Publishers[0].BannedTill > S.Moderators[1].BannedTill, "BannedTill timestamp did not change.");

		// TODO: Banish voting test. Waiting for implementation: ProposalVoting.cs:187
	}

}

