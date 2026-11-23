using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NativeImport;
using O;
using Uccs.Fair;
using Uccs.Net;
using static System.Net.Mime.MediaTypeNames;

namespace TestAutomation.Tests.Fair;

/// <summary>
/// Operation space/time fee validation tests.
/// - Site creation, 1 year.
/// - Site creation, 7 years.
/// - Site top-up.
/// - Author creation, 1 year.
/// - Author creation, 6 years.
/// - Author top-up.
/// - Product creation.
/// - Product update: setting a title to a single character.
/// - Product update: setting a title to a 2 character string.
/// - Product update: setting a title back to a 1 character string.
/// - Proposal to publish a product.
/// - Site user registration.
/// - Site category creation.
/// - File creation.
/// - File deletion.
/// </summary>
[TestType(TestType.Generic)]
class FeeValidation : FairTest
{
	Actor siteCreator;

	const uint genericYearFeeSt = 100 * 365;

	Store S => McvFind<Store>(FindFairUser(siteCreator.User.Name).ModeratedStores[0]);
	Author Au => McvFind<Author>(FindFairUser(siteCreator.User.Name).Id);


	public FeeValidation() : base("Fee Validation Test") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		siteCreator = CreateActor(topUp: true);
	}

	public override void Logic()
	{
		// Site creation, 1 year.
		var diff = GetSpacetimeDiff(siteCreator.User, () => CreateStore(siteCreator, "Site", 1));
		Assert(diff == -genericYearFeeSt, $"Expected site creation fee is {genericYearFeeSt}byte/years, but {diff / 365} were deducted.");

		////Site creation, 7 years.
		short years = 7;
		diff = GetSpacetimeDiff(siteCreator.User, () => CreateStore(siteCreator, "Site", years));
		Assert(diff == -genericYearFeeSt * years, $"Expected site creation fee is {genericYearFeeSt}byte/years, but {diff / 365} were deducted.");

		// Site top-up.
		// We don't use 'GetCurrencyDiff' because it only works with accounts, not with sites, maybe TODO?
		(long st, long e) = (130500230, 1326801);
		(long stDiff, long eDiff) = GetSiteTokenDiff(S.Id, () => 
			NetExec(Oses[0], $"utility t from={McvTable.User}/{Net.Father0Id} to={FairTable.Store}/{S.Id} e={e} st={st} by={Net.Father0Name}")
		);
		Assert(st == stDiff && e == eDiff, "Bad site top-up.");

		// Author creation, 1 year.
		stDiff = GetSpacetimeDiff(siteCreator.User, () => CreateAuthor(siteCreator, "Author", years: 1));
		Assert(stDiff == -genericYearFeeSt, $"Bad author creation fee.");

		AutoId authorId = FindFairUser(siteCreator.User.Name).Authors[0];

		// Author creation, 6 years.
		years = 6;
		stDiff = GetSpacetimeDiff(siteCreator.User, () => CreateAuthor(siteCreator, "Author", years));
		Assert(stDiff == -genericYearFeeSt * years, $"Bad author creation fee.");

		// Author top-up.
		(st, e) = (13050230, 1326801);
		(stDiff, eDiff) = GetAuthorTokenDiff(authorId, () => 
			NetExec(Oses[0], $"utility t from=User/{Net.Father0Id} to=Author/{authorId} e={e} st={st} by={Net.Father0Name}")
		);
		Assert(st == stDiff && e == eDiff, "Bad author top-up.");

		// Product creation.
		(stDiff, eDiff) = GetAuthorTokenDiff(authorId, () =>
			NetExec(siteCreator.Os, $"product c author={authorId} title=Title type={ProductType.Software} by={siteCreator.User.Name}")
		);
		Assert(stDiff == -genericYearFeeSt, "Bad product creation fee.");

		// Settings tags.
		(stDiff, eDiff) = GetAuthorTokenDiff(authorId, () =>
			NetExec(siteCreator.Os, $"product u id={McvFind<Author>(authorId).Products[^1]} definition={Token.Tags}=some_tag by={siteCreator.User.Name}")
		);
		Assert(stDiff == -365 * 8); // New tag has 8 characters -> 8 byte/days each day.
		// TODO krs One more tags change.

		// Proposal to publish a product. PublicationCreations proposal is approved by any moderator -> is discussion -> site pays for the operation.
		(stDiff, eDiff) = GetAuthorTokenDiff(authorId, () =>
			Propose(siteCreator.Os, S.Id, authorId, new PublicationCreation { Product = McvFind<Author>(authorId).Products[0] }, siteCreator.User.Name, Uccs.Fair.Role.Candidate)
		);
		Assert(stDiff == -genericYearFeeSt, $"ER: {-genericYearFeeSt}; AR: {stDiff}");

		// Site user registration. AFAIK always free.
		stDiff = GetSpacetimeDiff(siteCreator.User, () =>
			Propose(siteCreator.Os, S.Id, siteCreator.User.Id, new UserRegistration(), siteCreator.User.Name, role: Uccs.Fair.Role.User)
		);
		// User registration is free!
		Assert(stDiff == 0);

		// Category creation.
		// Site should fund this operation, not creator of the proposal (it is a "discussion", see `ProposalVoting.cs:66:Execute()`) - verification.
		(stDiff, _) = GetSiteTokenDiff(S.Id, () =>
			Propose(siteCreator.Os, S.Id, siteCreator.User.Id, new CategoryCreation { Title = "Misc" }, siteCreator.User.Name, role: Uccs.Fair.Role.Moderator)
		);
		Assert(stDiff == -genericYearFeeSt);
		stDiff = GetSpacetimeDiff(siteCreator.User, () =>
			Propose(siteCreator.Os, S.Id, siteCreator.User.Id, new CategoryCreation { Title = "Misc" }, siteCreator.User.Name, role: Uccs.Fair.Role.Moderator)
		);
		Assert(stDiff == 0);

		// TODO: What if last mod votes himself out?

		// File creation fee validation.
		string filePath = GetTestImagePath();
		long fileSpaceTime = GetFileSpacetimeRequirement(filePath);
		TopUpAuthor(authorId, fileSpaceTime);
		(stDiff, _) = GetAuthorTokenDiff(authorId, 
			() => CreateAuthorFile(siteCreator, authorId, filePath, FairMime.ImageJpg));
		Assert(stDiff == -fileSpaceTime);

		// File deletion refund validation.
		(stDiff, _) = GetAuthorTokenDiff(authorId, 
			() => DestroyFile(siteCreator, McvFind<Author>(authorId).Files[^1]));
		// Subtract one day, we never refund the day of creation operation.
		Assert(stDiff == fileSpaceTime - GetFileSpacetimeRequirement(filePath, 1));
		
		// Settings author description.
		string desc = "Big beautiful title";
		(stDiff, _) = GetAuthorTokenDiff(authorId,
			() => NetExec(siteCreator.Os, $"author info id={authorId} description=\"{desc}\" by={siteCreator.User.Name}"));
		Assert(stDiff == -desc.Length * 365);


		// Setting author title.
		string title = "Big beautiful title";
		NetExec(siteCreator.Os, $"author info id={authorId} title=\"{title}\" by={siteCreator.User.Name}");
		Author au = McvFind<Author>(authorId);
		(stDiff, _) = GetAuthorTokenDiff(authorId,
			() => NetExec(siteCreator.Os, $"author info id={authorId} title=\"{title}\" by={siteCreator.User.Name}"));
		Assert(stDiff == -(title.Length * 365 - au.Title.Length * (365-1)));

		// Setting author reference.
		string newRefText = "My stinky link";
		string newRefUri = "google.com.org.uk.abob";
		(stDiff, _) = GetAuthorTokenDiff(authorId,
			() => NetExec(siteCreator.Os, $"author info id={authorId} ref={{text=\"{newRefText}\" uri={newRefUri}}} by={siteCreator.User.Name}"));
		Assert(stDiff == -(newRefText.Length + newRefUri.Length) * 365);
	}
}

