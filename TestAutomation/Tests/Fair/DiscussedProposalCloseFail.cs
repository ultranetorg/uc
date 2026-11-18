using System.Collections.Generic;
using System.Diagnostics;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This test simulates a site description dispute, where 6 participants have a conversation using dispute comments. 
/// They comment and vote, change their comments. 2 positive votes are cast, and 3 negative after.
/// It is expected for the vote option that reaches half of the votes first to be accepted (this way tie is impossible). Note: this should be the case only 
/// for proposals with even voters, when the number of voters is uneven - ties are impossible, hence \"true\" majority of votes is expected.
/// </summary>
[TestType(TestType.Generic)]
class DiscussedProposalCloseFail : FairTest
{
	List<Actor> authors;
	List<Actor> mods = [];

	Store S => McvFind<Store>(FindFairUser(mods[0].User.Name).ModeratedStores[0]);

	public DiscussedProposalCloseFail() : base("Discussed Proposal Close Fail Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		authors = CreateSiteWithAuthors(6);
		mods.Add(authors[0]);
		mods.Add(StoreAddNewMod(S.Id, authors));
		mods.Add(StoreAddNewMod(S.Id, authors));

		//ChangeSitePolicy(S.Id, authors, FairOperationClass.SiteTextChange, ApprovalPolicy.PublishersMajority);
		ChangeStorePolicy(S.Id, authors, FairOperationClass.StoreInfoUpdation, ApprovalRequirement.PublishersMajority);
	}

	public void Logic()
	{
		Propose(authors[0].Os, S.Id, FindFairUser(authors[0].User.Name).Authors[^1],
			//new Uccs.Fair.SiteTextChange
			new Uccs.Fair.StoreInfoUpdation
			{
				Description = "Best site in the world"
			}, authors[0].User.Name, role: Uccs.Fair.Role.Publisher);
		Vote(S.Id, [authors[0]], Role.Publisher); // Voting for his own dispute.

		NetExec(authors[2].Os, [new ProposalCommentCreation {
			Proposal = S.Proposals[^1],
			Author = FindFairUser(authors[2].User.Name).Authors[0],
			Text = "WE ARE THE BEST, HELL YEAH!"
		}], authors[2].User.Name);

		// Correcting a comment with ProposalCommentEdit.
		string editedCommentText = "Greate idea :)";
		Proposal p = McvFind<Proposal>(S.Proposals[^1]);
		NetExec(authors[2].Os, [new ProposalCommentEdit {
			Comment = p.Comments[^1],
			Text = editedCommentText
		}], authors[2].User.Name);
		Vote(S.Id, [authors[2]], Role.Publisher);

		// Verify comment editing.
		Assert(McvFind<ProposalComment>(p.Comments[^1]).Text == editedCommentText);

		NetExec(authors[4].Os, [new ProposalCommentCreation {
			Proposal = S.Proposals[^1],
			Author = FindFairUser(authors[4].User.Name).Authors[0],
			Text = "The description surely should contain hint about what exactly we distribute here. So, voting NO."
		}], authors[4].User.Name);
		Vote(S.Id, [authors[4]], Role.Publisher, vote: (sbyte)SpecialChoice.Neither);

		NetExec(authors[3].Os, [new ProposalCommentCreation {
			Proposal = S.Proposals[^1],
			Author = FindFairUser(authors[3].User.Name).Authors[0],
			Text = "We have much more important things to do now, rather then playing with a site description."
		}], authors[3].User.Name);
		Vote(S.Id, [authors[3]], Role.Publisher, vote: (sbyte)SpecialChoice.Neither);

		NetExec(authors[5].Os, [new ProposalCommentCreation {
			Proposal = S.Proposals[^1],
			Author = FindFairUser(authors[5].User.Name).Authors[0],
			Text = "No need to change it, it's fine."
		}], authors[5].User.Name);
		Vote(S.Id, [authors[5]], Role.Publisher, vote: (sbyte)SpecialChoice.Neither);

		Assert(S.Proposals.Length == 0);
		Assert(string.IsNullOrEmpty(S.Description));
	}
}

