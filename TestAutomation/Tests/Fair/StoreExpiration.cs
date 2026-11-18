using System;

using Uccs.Net;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This tests verifies expected effects of store expiration.
/// According to @Maximion The Great, effects are:
/// - Nothing gets deleted.
/// - No store operations can be executed. Loss of control.
/// - "Names can be taken" (TODO @krs Not sure about this one, need to check.)
/// </summary>
[TestType(TestType.CommandLine)]
class StoreExpiration : FairTest
{
	Actor a1, a2;
	AutoId storeId;
	AutoId unpublishedProductId;
	AutoId publishedProductId;
	AutoId categoryId;

	public StoreExpiration() : base("Store Expiration Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		storeId = CreateStore(a1, "Store Badabor", years: 1, topUp: true);

		ProposeStoreCategoryCreation(a1, storeId, "Default");
		categoryId = McvFind<Store>(storeId).Categories[^1];

		a2 = CreateActor(topUp: true);
		AutoId authorId = CreateAuthor(a2, "title", years: 1, topUp: true);
		// An unpublished product for which publishing will be attempted after store expiration.
		unpublishedProductId = CreateProduct(a2.Os, authorId, a2.User.Name);

		// Published product in the store. Will be inspected after store expiration to verify 
		// expected behaviour.
		publishedProductId = CreateProduct(a2.Os, authorId, a2.User.Name);
		Propose(a1.Os, storeId, a1.User.Id,
			new PublicationCreation(publishedProductId),
			a1.User.Name, role: Uccs.Fair.Role.Moderator);
		AutoId pub = McvFind<Store>(storeId).UnpublishedPublications[^1];
		NetExec(a2.Os, $"publication p id={pub} publish=yes by={a2.User.Name}");
		Propose(a1.Os, storeId, a1.User.Id,
			new PublicationPublish(){Publication = pub, Category = categoryId},
			a1.User.Name, role: Uccs.Fair.Role.Moderator);
		Assert(McvFind<Store>(storeId).PublicationsCount == 1);
	}

	public void Logic()
	{
		TimeFastForward(TimeSpan.FromDays(366)); // Just over a year, enough to expire the store.

		// Any attempts to create proposals on an expired store are expected to fail.
		// TODO @krs Add missing proposal types, would be great to have every possible type.
		//		Or maybe that's not needed, because at the end of the day they all are the 
		//		same operation: a proposal?
		ProposeStoreCategoryCreation(a1, storeId, "Late Category", expectFail: true);
		Propose(a1.Os, storeId, a1.User.Id, 
			new PublicationCreation(unpublishedProductId), 
			a1.User.Name, role: Uccs.Fair.Role.Moderator, expectFailure: true);
		Propose(a1.Os, storeId, a1.User.Id,
			new StoreInfoUpdation
			{
				Description = "New description"
			}, a1.User.Name, Uccs.Fair.Role.Moderator, expectFailure: true);
		Propose(a1.Os, storeId, a1.User.Id,
			new StoreModeratorAddition
			{
				Candidates = [a2.User.Id]
			}, a1.User.Name, role: Uccs.Fair.Role.Moderator, expectFailure: true);

		// According to @Maximion nothing gets deleted from store when it expires.
		// Lets indirectly check this by ensuring that published publication has
		// it's properties intact.
		Publication publication = McvFind<Publication>(McvFind<Product>(publishedProductId).Publications[0]);
		Assert(publication.Category == categoryId);
		Assert(publication.Deleted == false);
		Assert(publication.IsPublished);
		Assert(publication.Store == storeId);
	}

}

