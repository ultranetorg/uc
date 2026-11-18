using System;
using System.Collections.Generic;

using Uccs.Net;
using Uccs.Fair;
using System.Diagnostics;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// Site with a publisher and a publication is created. The publication is then deleted. Each publication lifetime state is verified.
/// </summary>
[TestType(TestType.Generic)]
class PublicationRemoval : FairTest
{
	List<Actor> authors = [];
	List<Actor> mods = [];

	AutoId siteId;
	Store S => McvFind<Store>(siteId);

	AutoId category;

	public PublicationRemoval() : base("Publication Removal Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		mods.Add(CreateActor(topUp: true));
		siteId = CreateStore(mods[0], "Site", years: 1, topUp: true);
		category = CreateStoreCategory(siteId, mods, "Category");

		authors.Add(StoreAddNewAuthor(siteId, mods[0]));
	}

	public void Logic()
	{
		Assert(S.PublicationsCount == 1);
		Assert(S.UnpublishedPublications.Length == 1);
		Assert(S.ChangedPublications.Length == 0);

		AutoId pubId = McvFind<Product>(McvFind<Author>(FindFairUser(authors[0].User.Name).Authors[0]).Products[0]).Publications[0];
		Propose(mods[0].Os, siteId, mods[0].User.Id, new PublicationPublish { Publication = pubId, Category = category }, mods[0].User.Name, Uccs.Fair.Role.Moderator);

		Assert(S.PublicationsCount == 1);
		Assert(S.UnpublishedPublications.Length == 0);
		Assert(S.ChangedPublications.Length == 0);

		Propose(mods[0].Os, siteId, mods[0].User.Id, new PublicationDeletion { Publication = pubId }, mods[0].User.Name, Uccs.Fair.Role.Moderator);

		Assert(S.PublicationsCount == 0);
		Assert(S.UnpublishedPublications.Length == 0);
		Assert(S.ChangedPublications.Length == 0);
	}

}

