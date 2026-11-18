using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Uccs.Net;
//using Uccs.Uos;
using Uccs.Rdn;

namespace TestAutomation.Tests.Rdn;

using static System.Runtime.InteropServices.JavaScript.JSType;
using Rdn = Uccs.Rdn.Rdn;

/// <summary>
/// This testset is aimed to test correctness of transactions and operation fee calculations.
/// Ensuring that exact amount is deducted and received during transactions or other operations. TODO: More details!
/// This testset is huge and needs more explanation.
/// </summary>
[TestType(TestType.TestSet)]
class TransactionIntegrity : RdnTest
{
	Actor a, b;

	const long entityDayFee = 100;


	int fastForwardedDays = 0;

	public TransactionIntegrity() : base("Transaction Integrity Testset") { }

	public override void Init()
	{
		CreateMinimalNetwork();

		a = CreateActor(topUp: true);
		b = CreateActor(topUp: true);
	}

	public void Logic()
	{
		// TODO: Moment where resources exceed 4KB and free plan gets canceled,
		//		fee for all resources suddenly deducted.
		// TODO: Dependable?
		// TODO: Link creation now costs spacetime?


		// User creation is free: user creates itself, hence no transaction integrity 
		// can be checked.

		/* Utility transaction of spacetime. This one is simple, exactly the amount of sent
		 * spacetime should be deducted from the sender and received by the receiver.
		 * Perform transactions ensuring that balances are correct each step of the way.
		 */
		long sending = 100;
		(long bStPre, _) = GetBalance(b.User.Name);
		(long aStDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			UtilityTransactionU2U(a, b, st: sending)
		);
		(long bStAfter, _) = GetBalance(b.User.Name);
		Assert(aStDiff == -sending);
		Assert(bStAfter - bStPre == sending);


		/* Domain rent for a year costs differently depending on the 
		 * domain name length. Ensure that correct amount of spacetime is being 
		 * withdrawn. Check different timespans.
		 */
		/*					Years
		Domain Name Length  1			5			10
		1					10,000,000  50,000,000  100,000,000
		2					625,000		3,125,000   6,250,000
		3					123,457		617,284		1,234,568
		4					39,063		195,313		390,625
		5					16,000		80,000		160,000
		6					7,716		38,580		77,160
		7					4,165		20,825		41,649
		8					2,441		12,207		24,414
		9					1,524		7,621		15,242
		10					1,000		5,000		10,000
		*/
		string domainFree = "qwertyui"; // 8 chars.
		(long stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateDomain(a, domainFree, years: 1));
		// Free plan domain rent.
		// For domains longer or equal to 8 chars and rented only for 1 year.
		Assert(stDiff == 0); // 8 chars long for a year.

		string domain0 = "aboba"; // 5 chars.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateDomain(a, domain0, years: 1));
		Assert(stDiff == -16000); // 5 chars long for a year.

		string domain1 = "qwertyuy"; // 8 chars.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateDomain(a, domain1, years: 2));
		Assert(stDiff == 2 * -2441); // 8 chars long for 2 years.

		string domain2 = "qwertyuiopasdf"; // 14 chars.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateDomain(a, domain2, years: 3));
		Assert(stDiff == -1000 * 3); // 14 chars long for 3 years.

		string domain3 = "zazzz"; // 5 chars.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateDomain(a, domain3, years: 5));
		Assert(stDiff == -16000 * 5); // 5 chars long for 5 years.

		/* Resource creation. When resource is created, signer pays right-away 
		 * for its existence until the domain holding the resource expires.
		 * For example, if domain is rented for 5 years and user creates 
		 * resource in this domain - spacetime for 5 years of hosting that 
		 * resource will be deducted. The baseline cost of hosting an empty 
		 * resource is 100 byte/years. It's a mandatory fee. Hence creating 
		 * empty resource on our 5-year domain will cost 
		 * 100 * 5  = 500 byte/years. For each byte of data we add 1 byte/year 
		 * to the price. Lets say we create data resource of 6 bytes size. Now 
		 * annual cost will be (100 + 6) * 5 = 530 byte/years.
		 * 
		 * But there is a free plan. If a resource is created in the "free" 
		 * domain (rented for only one year and with a name of at least 8 
		 * characters), it is free until the total volume of resources 
		 * exceeds 4 KB.
		 */
		Ura res0 = new(domain3, "resource0");
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			NetExec(a.Os, $"resource create {res0} by={a.User.Name}")); // No data.
		Assert(stDiff == -entityDayFee * 365 * 5);

		Ura res1 = new(domain3, "resource1");
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateResource(a, res1.ToString(), $"{Meaning.Raw}{{hex=00}}"));
		Assert(stDiff == -(entityDayFee + 1) * 365 * 5); // +1 byte.

		Ura res2dep = new(domain3, "resource2"); // Dependable.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateResource(a, res2dep.ToString(), $"{Meaning.Raw}{{hex=0001020304}}", dependable: true));
		Assert(stDiff == -(entityDayFee + 5) * 365 * 5); // +5 bytes.

		Ura freeResource0 = new(domainFree, "resource0");
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			NetExec(a.Os, $"resource create {freeResource0} by={a.User.Name}")); // No data.
		Assert(stDiff == 0); // Empty resource => no fee because free plan.

		Ura freeResource1dep = new(domainFree, "resource1"); // Dependable.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			CreateResource(a, freeResource1dep.ToString(), $"{Meaning.Raw}{{hex=0101}}", dependable: true));
		Assert(stDiff == 0); // 2 byte resource => no fee because free plan.

		/* Resource exists in the domain until domain expires. In the previous 
		 * test we created resource at the same time as we acquired domain, 
		 * which means that resource lifetime == domain lifetime. This is not 
		 * always the case, resource can be created at any time. That means if 
		 * user creates a resource e.g. in the middle of domain's lifetime - 
		 * only spacetime from the time of the resource creation and until 
		 * domain expiration should be fined. Lets say user rents a domain for 
		 * a 1 year and creates a resource of 6 bytes size, 5 days before the 
		 * domain expiration. That means that resource will exist 5 days, and 
		 * user should pay for exactly 5 days of the resource existence. 
		 * Resource existence fee is 100 bytes per day. So, for an empty 
		 * resource it will be 100 * 5days = 500byte/days. Now lets say 
		 * resource is not empty, it contains data of 6 bytes. Now it will be 
		 * (100 + 6bytes) * 5days = 530byte/days
		*/

		// 360 days pass, 5 days of a year-long domain lifetime left.
		fastForwardedDays += 360;
		TimeFastForward(TimeSpan.FromDays(360));

		Ura shortLived = new(domain0, "short_lived");
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			CreateResource(a, shortLived.ToString(), $"{Meaning.Raw}{{hex=010203040506}}"));
		Assert(stDiff == -(entityDayFee + 6) * 5); // 6 bytes for 5 days
														// (5 days until domain expiration).

		/* When user destroys a resource - he should be refunded all unused 
		 * spacetime that he paid for in advance(because he paid for a 
		 * reasource until the end of the domain rent).
		 *
		 *     Domain   Resource   Resource                 Domain
		 *     Rented   Created    Destroyed               Expiration
		 *       |       |            v                       |
		 * *-----*-------*------------*-----------------------*--------> TIME
		 *               |             \                     /|
		 *               |              \-------------------/ |
		 *               |               Spacetime to refund  |
		 *                \                                  /
		 *                 \--------------------------------/
		 *                     Paid for resource creation
		 *
		 * To prevent spam and users destroying resource the same day it was 
		 * created for free - user is not refunded for the day when resource is
		 * destroyed. This means that if you destroy resource in the domain 
		 * that has 100 rent days left - only 100 - 1 = 99 days get refunded.
		 */
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			DeleteResource(a, shortLived));
		Assert(stDiff == (entityDayFee + 6) * (5 - 1)); // - 1 day because same day.

		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
		DeleteResource(a, freeResource0));
		Assert(stDiff == 0); // Free plan resource - created for free, so
							 // nothing to be refunded.

		/* Resources may be updated, and replacement data may differ in size; 
		 * if it is larger, the user pays for the additional space, and if it 
		 * is smaller, the user is refunded the difference. 
		 * In practice, the user is first refunded the allocated space for the 
		 * old data (excluding today) and then charged for the new resource 
		 * data (including today) in 2 steps.
		 */
		// Upgrading resource size: empty => 5 bytes.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			UpdateResource(a, res0, $"{Meaning.Raw}{{hex=0102030405}}"));
		// Domain where res0 is located was rented for 5 years, 360 days were
		// fast-forwarded since, that means 5 * 365 - 360 days left.
		Assert(stDiff == -5 * (5 * 365 - 360));

		// Upgrading resource size: 1 byte => 2 bytes.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			UpdateResource(a, res1, $"{Meaning.Raw}{{hex=0102}}"));
		// Domain where res1 is located was rented for 5 years, 360 days were
		// fast-forwarded since, that means 5 * 365 - 360 days left.
		// 1. Refund for the old, 1 byte resource:
		//			(5 * 365days - 360days - 1day) * 1byte = 1464 byte/days
		//                                    ^
		//                              Refunds exclude today

		// 2. Fee for the new, 2 byte resource:
		//			(5 * 365days - 360days) * 2bytes = 2930 byte/days
		// 3. Final expected fee:  2930 - 1464 = 1466
		Assert(stDiff == -1466);

		// Upgrading resource size: 5 bytes => 20 bytes.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			UpdateResource(a, res2dep, $"{Meaning.Raw}{{hex=0102030405060708090A0B0C0D0E0F1011121314}}"));
		// Domain where res2 is located was rented for 5 years, 360 days were
		// fast-forwarded since, that means 5 * 365 - 360 days left.
		// 1. Refund for the old, 1 byte resource:
		//			(5 * 365days - 360days - 1day) * 5byte = 7320 byte/days
		//                                    ^
		//                              Refunds exclude today

		// 2. Fee for the new, 2 byte resource:
		//			(5 * 365days - 360days) * 20bytes = 29300 byte/days
		// 3. Final expected fee:  29300 - 7320 = 21980
		Assert(stDiff == -21980);

		TimeFastForward(TimeSpan.FromDays(200)); // 200 days pass.
		fastForwardedDays += 200;

		// Downgrading resource size: 20 bytes => 3 bytes.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			UpdateResource(a, res2dep, $"{Meaning.Raw}{{hex=010203}}"));
		// Domain where res2 is located was rented for 5 years, 360 + 200 = 560
		// days were fast-forwarded since, that means 5 * 365 - 560 = 1265 days
		// left.
		// 1. Refund for the old, 1 byte resource:
		//			(1265days - 1day) * 20byte = 25280 byte/days
		//                                    ^
		//                              Refunds exclude today

		// 2. Fee for the new, 2 byte resource:
		//			1265days * 3bytes = 3795 byte/days
		// 3. Final expected difference: 25280 - 3795 = 21980
		Assert(stDiff == 21485);

		/* Transaction integrity test of the file and directory resources. Both these resource types represent a release Urrh.
		   Urr contains 70 characters: Urrh:/01D448AFD928065458CF670B60F5A594D735AF0172C8D67F22A81680132681CA
		   hence expected space byte/years fee is expected to be 70 bytes. 
		 */
		string filePath = CreateTestFile("file.txt");
		var lra = CreateRelease(a.Os, filePath);
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateResource(a, $"/{domain3}/my_file", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}"));
		// Default resource maintanance: 100 byte/days.
		// Urrh occuppies 70 byte/days.
		// 1265 days left until domain expiration.
		long expectedDiff = (entityDayFee + 70) * 1265;
		Assert(stDiff == -expectedDiff);
		File.Delete(filePath);

		string dirPath = CreateMinimalReleaseDirectory("testDir");
		lra = CreateRelease(a.Os, dirPath);
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			CreateResource(a, $"/{domain3}/my_dir", $"{Meaning.ExternalContent}/{ContentType.File}{{address={lra.Address}}}"));
		// Default resource maintanance: 100 byte/days.
		// Urrh occuppies 70 byte/days.
		// 1265 days left until domain expiration.
		expectedDiff = (entityDayFee + 70) * 1265;
		Assert(stDiff == -expectedDiff);
		Directory.Delete(dirPath, recursive: true);

		// Creation and deletion of links.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateLink(a, res0, res1));
		Assert(stDiff == -(5 * 365 - fastForwardedDays) * entityDayFee);

		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateLink(a, res1, res0, type: ResourceLinkType.Extra));
		Assert(stDiff == -(5 * 365 - fastForwardedDays) * entityDayFee);

		// Dependency link to a dependable resource.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			CreateLink(a, res1, res2dep, type: ResourceLinkType.Dependency));
		Assert(stDiff == -(5 * 365 - fastForwardedDays) * (entityDayFee + 3));
			// +3 bytes (res2dep resource volume is 3 bytes).

		// Deletion of a basic link.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			DeleteLink(a, res0, 0));
		Assert(stDiff == (5 * 365 - fastForwardedDays - 1) * entityDayFee);

		// Deletion of a dependency link to a dependable resource.
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () =>
			DeleteLink(a, res1, 1));
		Assert(stDiff == (5 * 365 - fastForwardedDays - 1) * (entityDayFee + 3));

		// Domain migration. Consumes additional 100 energy rating.
		int erBefore = FindUser(a.User.Name).EnergyRating;
		NetExec(a.Os, $"domain migrate ultranet wtld=org by={a.User.Name}");
		int erAfter = FindUser(a.User.Name).EnergyRating;
		Assert(erBefore - erAfter == 100 + 1);

		// Actor passes ownership of the domain to another actor.
		string domain = GetUnusedLongDomainName();
		CreateDomain(a, domain, 3);
		CreateResource(a, $"/{domain}/resource");

		(long bActorBeforeSt, _) = GetBalance(b.User.Name);
		(stDiff, _) = GetUserActionTokenDiff(a.User.Name, () => 
			NetExec(a.Os, $"domain s {domain} owner={b.User.Name} by={a.User.Name}"));
		(long bActorAfterSt, _) = GetBalance(b.User.Name);

		// Assert ownership transfer.
		Assert(FindDomain(domain).Owner == b.User.Id);
		Assert(stDiff == 0);
		Assert(bActorAfterSt - bActorBeforeSt == 0);
	}

}
