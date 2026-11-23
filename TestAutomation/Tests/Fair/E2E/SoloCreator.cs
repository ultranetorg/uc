using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// This e2e test represent a use case when a user creates account, site, site category, product, and then publishes the product 
/// on just created site, all by his own.
/// </summary>
[TestType(TestType.EndToEnd)]
class SoloCreator : FairTest
{
	public SoloCreator() : base("Solo Creator End To End Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();
	}

	public override void Logic()
	{
		Actor a = CreateActor(topUp: true);

		// Create an author entity.
		AutoId authorId = CreateAuthor(a, "My Publisher", years: 1, topUp: false);

		// Top-up author account.
		TopUpAuthor(authorId);

		// Create a product and it's definition.
		AutoId productId = CreateProduct(a.Os, authorId, a.User.Name, title: "MyProduct");


		// Create a site. Tops it up.
		AutoId siteId = CreateStore(a, "My Site", years: 1, topUp: true);

		// Create a category for a publication.
		AutoId categoryId = CreateStoreCategory(siteId, [a], "DefaultCategory");

		// Propose publication creation (automatically executed - only one moderator and same user as a proposal creator).
		Propose(a.Os, siteId, a.User.Id, new PublicationCreation { Product = productId }, a.User.Name, Uccs.Fair.Role.Moderator);

		// Propose publication publication(???) (automatically executed - only one moderator and same user as a proposal creator).
		AutoId unpub = McvFind<Store>(siteId).UnpublishedPublications[^1];
		Propose(a.Os, siteId, a.User.Id, new PublicationPublish() { Publication = unpub, Category = categoryId }, a.User.Name, Uccs.Fair.Role.Moderator);

		Author au = McvFind<Author>(authorId);
		Store s = McvFind<Store>(siteId);
		Assert(au.Products.Length == 1);
		Assert(s.PublicationsCount == 1);
		Assert(s.UnpublishedPublications.Length == 0);
		Assert(s.ChangedPublications.Length == 0);

		NetExec(Oses[1], $"site e {s.Id}");
		NetExec(Oses[1], $"product e {au.Products[0]}");
	}

}

