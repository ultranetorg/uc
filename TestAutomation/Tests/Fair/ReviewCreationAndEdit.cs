using System;
using System.Linq;

using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// - Review creation, its existence and content verified.
/// - Review edited, its existence and new content verified.
/// </summary>
[TestType(TestType.Generic)]
class ReviewCreationAndEdit : FairTest
{
	Actor mod;
	//Product prod;
	AutoId productId;

	Store S => McvFind<Store>(FindFairUser(mod.User.Name).ModeratedStores[0]);

	public ReviewCreationAndEdit() : base("Review Creation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		mod = CreateActor(topUp: true);
		AutoId siteId = CreateStore(mod, "Site", 1, topUp: true);
		Propose(mod.Os, S.Id, mod.User.Id, new CategoryCreation { Title = "Soft" }, mod.User.Name, Uccs.Fair.Role.Moderator);
		AutoId authId = CreateAuthor(mod, "AuthorBro", 1, topUp: true);
		productId = CreateProduct(mod.Os, authId, mod.User.Name, "FunnyProduct");

		// No need to vote for proposal - creator is a mod, and a single mod is needed to approve publication creation,
		// hence it's done automatically.
		Propose(mod.Os, S.Id, authId, new PublicationCreation { Product = productId }, mod.User.Name, Uccs.Fair.Role.Candidate);

		AutoId publication = McvFind<Product>(McvFind<Author>(authId).Products[0]).Publications[0];
		Propose(mod.Os, S.Id, mod.User.Id, new PublicationPublish { Publication = publication, Category = S.Categories[0] }, mod.User.Name, Uccs.Fair.Role.Moderator);
	}

	public override void Logic()
	{
		// Register 2 users.
		Actor reviewer1 = CreateActor(topUp: true);
		Propose(reviewer1.Os, S.Id, reviewer1.User.Id, 
			new UserRegistration(), reviewer1.User.Name, role: Uccs.Fair.Role.User);
		Vote(S.Id, [mod], Uccs.Fair.Role.Moderator);

		Actor reviewer2 = CreateActor(topUp: true);
		Propose(reviewer2.Os, S.Id, reviewer2.User.Id, 
			new UserRegistration(), reviewer2.User.Name, role: Uccs.Fair.Role.User);
		Vote(S.Id, [mod], Uccs.Fair.Role.Moderator);

		// Create a review.
		string reviewText = "This is a review text.";
		Product p = McvFind<Product>(productId);
		Propose(reviewer1.Os, S.Id, reviewer1.User.Id, 
			new ReviewCreation { Publication = p.Publications[0], Text = reviewText }, 
			reviewer1.User.Name, role: Uccs.Fair.Role.User);
		Vote(S.Id, [mod], Uccs.Fair.Role.Moderator);

		// Verify review.
		Publication pub = McvFind<Publication>(p.Publications[0]);
		Review r = McvFind<Review>(pub.Reviews[0]);
		Assert(pub.Reviews.Count == 1);
		Assert(r.Text == reviewText, "Review text does not match.");

		// Attempt to create second review of the same publication by a user.
		// Only one review per product is allowed for a user! Failure expected.
		reviewText = "Second illegal review text.";
		Propose(reviewer1.Os, S.Id, reviewer1.User.Id,
			new ReviewCreation { Publication = p.Publications[0], Text = reviewText }, 
			reviewer1.User.Name, role: Uccs.Fair.Role.User, expectFailure: true);

		// Review edit.
		string editedReviewText = "This is an edited review text.";
		Propose(reviewer1.Os, S.Id, reviewer1.User.Id, 
			new ReviewEdit { Review = r.Id, Text = editedReviewText }, 
			reviewer1.User.Name, role: Uccs.Fair.Role.User);
		Vote(S.Id, [mod], Uccs.Fair.Role.Moderator);

		r = McvFind<Review>(pub.Reviews[0]); // Refetch.
		Assert(r.Text == editedReviewText, "Review text does not match.");

		// Review edit attempt by another, not authorized user. Failure expected.
		string editedForeignReviewText = "This is a new edited review text.";
		Propose(reviewer2.Os, S.Id, reviewer2.User.Id,
			new ReviewEdit { Review = r.Id, Text = editedForeignReviewText }, 
			reviewer2.User.Name, role: Uccs.Fair.Role.User, expectFailure: true);

		r = McvFind<Review>(pub.Reviews[0]); // Refetch.
		Assert(r.Text != editedForeignReviewText);
	}

}

