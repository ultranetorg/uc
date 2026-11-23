using System;

using Uccs.Net;
using Uccs.Fair;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This tests verifies expected effects of author expiration.
/// </summary>
[TestType(TestType.CommandLine)]
class AuthorExpiration : FairTest
{
	Actor a1, a2;
	AutoId storeId, authorId, categoryId, unpublishedProductId, publishedProductId;

	public AuthorExpiration() : base("Author Expiration Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a1 = CreateActor(topUp: true);
		storeId = CreateStore(a1, "Store Badabor", years: 10, topUp: true);

		ProposeStoreCategoryCreation(a1, storeId, "Default");
		categoryId = McvFind<Store>(storeId).Categories[^1];

		a2 = CreateActor(topUp: true);
		authorId = CreateAuthor(a2, "title", years: 1, topUp: true);
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

	public override void Logic()
	{
		TimeFastForward(TimeSpan.FromDays(366)); // Just over a year, enough to expire author.

		// Author's attempt to change his own name. Expected to fail - author is expired.
		NetExec(a2.Os, $"author name id={authorId} newname=goofball by={a2.User.Name} aor=expectfailure");

		// Attempt to change author title/description with "info" command. Expected to fail - author is expired.
		NetExec(a2.Os, $"author info id={authorId} title=goofball description=\"New description\" by={a2.User.Name} aor=expectfailure");

		// Attempt to add a new author owner. Expected to fail - author is expired.
		NetExec(a2.Os, $"author security id={authorId} addowner={a1.User.Id} by={a2.User.Name} aor=expectfailure");

		// Attempt to renew an author. Expected to fail - author is expired.
		NetExec(a2.Os, $"author renew id={authorId} years=1 by={a2.User.Name} aor=expectfailure");

		// According to @Maximion nothing gets deleted when author expires.
		// Publication is not deleted on the site - it is present. This is expected according to @Maximion.
		Assert(McvFind<Store>(storeId).PublicationsCount == 1);
		Assert(McvFind<Product>(publishedProductId).Publications.Length == 1);
	}

}

