using System;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

using Uccs;
using Uccs.Net;
using Uccs.Rdn;
using Uccs.Fair;
using O;

using CommonUtils;
using JunitReporter;

using TestAutomation;

namespace TestAutomation.Tests.Fair;

[TestNetwork(TestNetwork.Fair)]
abstract class FairTest : TestBase
{
	Dictionary<Os, FairApiClient> Clients = [];

	protected override McvCli GetCli(Os os) => new FairCli(os.FindNode<FairNode>().NexusSettings, os.FindNode<FairNode>().Settings, GetApi(os)) { Application = os.Name };
	protected override McvNode GetNode(Os os) => os.FindNode<FairNode>();
	protected override McvNodeSettings CreateSettings(Os os) => new FairNodeSettings(Path.Join(os.Profile, typeof(FairNode).FullName));

	public FairTest(string testName) : base(testName) 
	{
		base.Init();

		Net = Uccs.Fair.Fair.TA;
		Group = new OsGroup(Path.Combine(G.Developer.Tmp, this.GetType().Name), Net.Initials);
	}

	protected override void Configure(Os os, Xon args, McvNodeSettings settings)
	{
		var s = settings as FairNodeSettings;

		s.DataPath = Path.Join(G.Developer.Root, "UC", nameof(Fair), "Data");

		if(s.Web != null)
		{
			s.Web.ListenUrl = $"http://{settings.Peering.Endpoint.IP}:1080";
		}
	}

	public override McvNode StartNode(Os os, McvNodeSettings settings)
	{
		var f = Flow.CreateNested(os.Name, new Log());

		f.WorkDirectory = Path.Join(os.Profile, typeof(FairNode).FullName);

		return new FairNode(Net.Zone, os.Nexus.Settings, settings as FairNodeSettings, Clock, f);
	}

	public override List<Os> CreateMinimalNetwork()
	{
		List<Os> oses = [];
	
		oses.Add(Group.CreateOs("U0", "f=2 nn", Net.Father0EP.IP));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U1", "f=2 nn", Group.Initials[1]));
		CreateNode(oses[^1], "mcv chain g=2 s run");
		oses.Add(Group.CreateOs("U2", "f=2 nn", Group.Initials[2]));
		CreateNode(oses[^1], "mcv chain g=2 s run");

		SyncAllPeering();
		// Uoses won't start synchronizing until there is a new round full of transactions to be synced.
		// We trick uoses to sync by this.
		//TransactUntilSync();

		return oses;
	}

	public override Os CreateOsNodePair()
	{
		Os os = Group.CreateOs($"U{Oses.Count}", "nn", GetUnusedIp());
		CreateNode(os, "run");
		return os;
	}

	protected FairUser FindFairUser(string name)
	{
		var m = Oses[0].FindNode<McvNode>().Mcv;

		lock(m.Lock)
		{
			//return m.Users.Find(name, m.LastConfirmedRound.Id) as FairUser;
			return m.Users.Latest(name) as FairUser;
		}
	}

	/// <summary>
	/// Creates site with authors on a single or different UOSes for each author.
	/// If UOS to use is not provided - for each new account new UOS will be created.
	/// First created new UOS+account will be a site creator and a first author. All next accounts - just authors.
	/// </summary>
	/// <param name="authCount">Number of authors that should be created. Each will create publication, it will get approved and added to the site.</param>
	/// <param name="u">UOS to use. If provided - all the accounts will be created on this single UOS. Otherwise - for each account new UOS will be created(more realistic case)</param>
	/// <returns>List of created accounts. First account - site creator.</returns>
	public List<Actor> CreateSiteWithAuthors(uint authCount, in Os os = null)
	{
		List<Actor> authors = [];
		Actor siteOwner = null;
		AutoId siteId = null;

		for(int i = 0; i < authCount; ++i)
		{
			Os currentOs = os;
			currentOs ??= CreateOsNodePair();

			if(i == 0)
			{
				siteOwner = CreateActor(currentOs, topUp: true);
				siteId = CreateStore(siteOwner, "Site", 1, topUp: true);

				AutoId authorId = CreateAuthor(siteOwner, "Author", 1, topUp: true);
				AutoId productId = CreateProduct(siteOwner.Os, authorId, siteOwner.User.Name);
				Propose(siteOwner.Os, siteId, authorId, new PublicationCreation(productId), 
					siteOwner.User.Name, role: Uccs.Fair.Role.Candidate);
				authors.Add(siteOwner);
			}
			else
				authors.Add(StoreAddNewAuthor(siteId, siteOwner, currentOs));
		}

		return authors;
	}

	/// <summary> Creates authors (through publication) on an existing site.
	/// Authors are located either on a single UOS, or each on a new UOS. </summary>
	/// <param name="authCount"> Number of authors to create. </param>
	/// <param name="s"> Site where authors should be joined. </param>
	/// <param name="siteCreator"> Uos that holds an account of the site creator. </param>
	/// <param name="os"> UOS where new author accounts should be located. `null` to create a new UOS for each new author. </param>
	/// <returns> List of author accounts created. </returns>
	public List<Actor> SiteAddNewAuthors(uint authCount, AutoId siteId, Actor mod, Os? os = null)
	{
		List<Actor> newAuthors = new();

		for(int i = 0; i < authCount; ++i)
			newAuthors.Add(StoreAddNewAuthor(siteId, mod, os));

		return newAuthors;
	}

	public Actor StoreAddNewAuthor(AutoId storeId, Actor mod, in Os host = null)
	{
		Os os = host;
		Actor a = CreateActor(topUp: true);
		AutoId authorId = CreateAuthor(a, "Author", 1, topUp: true);
		AutoId productId = CreateProduct(a.Os, authorId, a.User.Name);
		Propose(a.Os, storeId, authorId, new PublicationCreation { Product = productId }, a.User.Name, role: Uccs.Fair.Role.Candidate);
		Store s = McvFind<Store>(storeId);
		Vote(storeId, [mod], Uccs.Fair.Role.Moderator);

		return a;
	}

	public Actor StoreAddNewMod(AutoId siteId, List<Actor> authors, Os os = null)
	{
		Actor a = CreateActor(topUp: true);

		// Any of the site authors proposes to add user as a mod. We use the first author - with index 0.
		Propose(authors[0].Os, siteId, FindFairUser(authors[0].User.Name).Authors[0],
			new StoreModeratorAddition
			{
				Candidates = [a.User.Id]
			}, 
		authors[0].User.Name, role: Uccs.Fair.Role.Publisher);

		if(authors.Count > 1) Vote(siteId, authors, Uccs.Fair.Role.Publisher);

		return a;
	}

	protected void ProposeStoreCategoryCreation(Actor proposalCreator, 
												AutoId storeId, 
												string categoryTitle, 
												Uccs.Fair.Role proposalCreatorRole = Uccs.Fair.Role.Moderator,
												bool expectFail = false)
	{
		Propose(proposalCreator.Os, storeId, proposalCreator.User.Id,
			new CategoryCreation { Title = categoryTitle }, proposalCreator.User.Name, proposalCreatorRole, expectFail);
	}

	/// <summary> Creates a site category and returns its ID. </summary>
	public AutoId CreateStoreCategory(AutoId siteId, List<Actor> mods, string title)
	{
		Propose(mods[0].Os, siteId, mods[0].User.Id,
			new CategoryCreation { Title = title }, mods[0].User.Name, Uccs.Fair.Role.Moderator);
		if(mods.Count > 1) Vote(siteId, mods, Uccs.Fair.Role.Moderator, all: true);
		return McvFind<Store>(siteId).Categories[^1];
	}

	public void Vote(AutoId siteId, List<Actor> voters, Uccs.Fair.Role votersRole, bool all = false, sbyte vote = 0)
	{
		Store s = McvFind<Store>(siteId);
		foreach(Actor a in all ? voters : voters[..((voters.Count + 1) / 2)])
		{
			AutoId voterId = null;
			if(votersRole == Uccs.Fair.Role.Moderator || votersRole == Uccs.Fair.Role.User)
				voterId = FindFairUser(a.User.Name).Id;
			else if(votersRole == Uccs.Fair.Role.Publisher)
				voterId = FindFairUser(a.User.Name).Authors[0];
			else throw new("Not implemented.");

			NetExec(a.Os, [new ProposalVoting(s.Proposals[^1], voterId, vote)], a.User.Name);
		}
	}

	protected object Propose(Os os, AutoId storeId, AutoId proposalCreatorId, VotableOperation p, string username, Uccs.Fair.Role role, bool expectFailure = false)
	{
		return NetExec(os, [new ProposalCreation(storeId, proposalCreatorId, role, p)], username, expectFailure);
	}

	protected object ProposeOpts(Os os, AutoId siteId, AutoId proposalCreatorId, Option[] options, string username, Uccs.Fair.Role role)
	{
		return NetExec(os, [new ProposalCreation(siteId, proposalCreatorId, role, options)], username);
	}

	/* @Todo 
	 This is not a good method. It assumes 2 things at once: 
	 - That argument `pubs` contains all the actors who are publishers on the site.
	 - That all these `pubs` publishers have only one author created.
	 This could be fixed by embedding those "mods" and "authors" lists that I usually declare 
	 in the test class into base classes. That would potentially chain alot of changes tho.. need 
	 to think about this more.
	*/
	public void ChangeStorePolicy(AutoId siteId, List<Actor> pubs, FairOperationClass operationClass, ApprovalRequirement approval)
	{
		Store s = McvFind<Store>(siteId);
		Survey ps = s.FindApprovalPolicyChangeSurvey(operationClass);
		sbyte desiredIdx = ps.FindIndex(approval);
		if(ps.LastWin == desiredIdx) return; // Desired policy is already in place.

		for(int i = 0; i < pubs.Count / 2 + (pubs.Count & 1); ++i)
		{
			s = McvFind<Store>(s.Id);
			NetExec(pubs[i].Os, [new SurveyVoting {
				Store = s.Id,
				Publisher = FindFairUser(pubs[i].User.Name).Authors[0],
				Survey = s.FindApprovalPolicyChangeSurvey(operationClass).Id,
				Choice = s.FindApprovalPolicyChangeSurvey(operationClass).FindIndex(approval)
			}], pubs[i].User.Name);

		}
	}

	public AutoId CreateStore(Os os, string title, short years, string username, bool topUp = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(os, $"store c years={years} title=\"{title}\" by={username} aor={aor.ToString().ToLower()}");
		if(aor == ActionOnResult.ExpectFailure) return null;
		AutoId siteId = FindFairUser(username).ModeratedStores[^1];
		if(topUp) TopUpStore(siteId);

		return siteId;
	}

	public AutoId CreateStore(Actor a, string title, short years, bool topUp = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateStore(a.Os, title, years, a.User.Name, topUp, aor);
	}


	/// <summary> This renews only first site. </summary>
	public AutoId SiteRenew(Actor a, short years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		AutoId siteId = FindFairUser(a.User.Name).ModeratedStores[0];
		NetExec(a.Os, $"site r {siteId} years={years} by={a.User.Name} aor={aor.ToString().ToLower()}");
		return siteId;
	}

	/// <summary>
	/// Creates a product by running 'product c' and then updates the definition of the created product by 'product u', setting the title.
	/// </summary>
	/// <param name="os"> OS where user (owner of the author account) lives </param>
	/// <param name="authorId"> Id of the author account that will create a product </param>
	/// <param name="username"> Username of the user that owns the author account </param>
	/// <param name="title"> Title of the created product(is set by 'product u' command) </param>
	/// <param name="prodType"> Product type: None/Software/Movie/Music/Book/Game </param>
	/// <returns> Id of the created product entity </returns>
	public AutoId CreateProduct(Os os, AutoId authorId, string username, string title = "Product", ProductType prodType = ProductType.Software, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(os, $"product c author={authorId} type={ProductType.Software} title={title} by={username} aor={aor.ToString().ToLower()}");
		AutoId productId = McvFind<Author>(authorId).Products[^1];
		NetExec(os, $"product u id={productId} definition=\"{Token.Tags}=hype\" by={username}");
		return productId;
	}

	public AutoId CreateAuthor(Os os, string title, short years, string username, bool topUp = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(os, $"author c title=\"{title}\" years={years} by={username} aor={aor.ToString().ToLower()}");
		if(aor != ActionOnResult.RetryUntilConfirmed) return null;
		AutoId authorId = FindFairUser(username).Authors[^1];
		if(topUp) TopUpAuthor(authorId);

		return authorId;
	}

	public AutoId CreateAuthor(Actor a, string title, short years, bool topUp = false, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return CreateAuthor(a.Os, title, years, a.User.Name, topUp, aor);
	}

	public object AuthorRenew(Os os, AutoId authId, short years, string username, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(os, $"author r {authId} years={years} by={username} aor={aor.ToString().ToLower()}");
	}

	/// <summary> This renews only latests created author of an actor. </summary>
	public object AuthorRenew(Actor a, short years, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		return NetExec(a.Os, $"author r {FindFairUser(a.User.Name).Authors[^1]} years={years} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	public void TopUpAuthor(AutoId authId, long st = 1000000, long e = 10000)
	{
		NetExec(Oses[0], $"utility t from=User/{Net.Father0Id} to=Author/{authId} st={st} e={e} by={Net.Father0Name}");
	}

	public void TopUpStore(AutoId siteId, uint st = 1000000, uint e = 10000)
	{
		NetExec(Oses[0], $"utility t from={McvTable.User}/{Net.Father0Id} to={FairTable.Store}/{siteId} st={st} e={e} by={Net.Father0Name}");
	}

	public object ProductUpdate(Os os, AutoId prodId, string definition, PublicKey signer, TransactionStatus await = TransactionStatus.Confirmed)
	{
		return NetExec(os, $"product u {prodId} definition={definition} signer={signer} await={await.ToString().ToLower()}");
	}

	public AutoId CreateAuthorFile(Actor a, AutoId authorId, string filePath, FairMime mime = FairMime.None, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"file c owner={FairTable.Author}/{authorId} path={filePath} mime={mime} by={a.User.Name} aor={aor.ToString().ToLower()}");
		return McvFind<Author>(authorId).Files[^1];
	}

	public void DestroyFile(Actor a, AutoId fileId, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"file x id={fileId} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	public void SetAuthorAvatar(Actor a, AutoId authorId, AutoId fileId, ActionOnResult aor = ActionOnResult.RetryUntilConfirmed)
	{
		NetExec(a.Os, $"author avatar {authorId} file={fileId} by={a.User.Name} aor={aor.ToString().ToLower()}");
	}

	FairApiClient GetApi(Os os)
	{
		if(Clients.TryGetValue(os, out var c))
			return c;
		else
		{
			return Clients[os] = new FairApiClient(Api.ForNode(Net, os.IP));
		}
	}

	public long GetSpacetimeDiff(User u, Action a) {
		long stBefore = FindUser(u.Name).Spacetime;
		a?.Invoke();
		long stAfter = FindUser(u.Name).Spacetime;
		return stAfter - stBefore;
	}

	public (long, long) GetBalance(AutoId authorId)
	{
		Author au = Oses[0].McvFind<Author>(authorId);
		return (au.Spacetime, au.Energy);
	}

	public (long stDiff, long eDiff) GetCurrencyDiff(AutoId authorId, Action a)
	{
		(long st, long e) = GetBalance(authorId);
		a();
		(long st_after, long e_after) = GetBalance(authorId);
		return (st_after - st, e_after - e);
	}

	public (long stDiff, long eDiff) GetSiteTokenDiff(AutoId siteId, Action a)
	{
		Store s_ = McvFind<Store>(siteId);
		(long st, long e) = (s_.Spacetime, s_.Energy);
		a?.Invoke();
		s_ = McvFind<Store>(siteId);
		(long st_after, long e_after) = (s_.Spacetime, s_.Energy);
		return (st_after - st, e_after - e);
	}

	public (long stDiff, long eDiff) GetAuthorTokenDiff(AutoId authId, Action a) {
		Author au = McvFind<Author>(authId);
		(long st, long e) = (au.Spacetime, au.Energy);
		a?.Invoke();
		au = McvFind<Author>(authId);
		return (au.Spacetime - st, au.Energy - e);
	}

	public void WaitForSync()
	{
		while(true)
		{
			Dictionary<Os, Synchronization> statuses = [];
			Console.WriteLine("Syncing... ");
			foreach(Os os in Oses)
			{
				statuses[os] = os.FindNode<FairNode>().Peering.Synchronization;
				Console.WriteLine($"UOS: {os.Name}, Status: {statuses[os]}");
			}

			foreach(var pair in statuses)
			{
				if(pair.Value != Synchronization.Synchronized)
				{
					Thread.Sleep(300);
					continue;
				}
			}
			break;
		}

	}

	public static void WaitForSync(Os os)
	{
		while(true)
		{
			bool synced = os.FindNode<FairNode>().Peering.Synchronization == Synchronization.Synchronized;
			if(synced) break;
			Thread.Sleep(100);
			Console.Write("Syncing... ");
		}

	}

	public static void AssertNoProposals(Store s, string failMsg = "Proposals exist.")
	{
		Assert(s.Proposals.Length == 0, failMsg);
	}


}
