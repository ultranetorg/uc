# Tests

### Author Expiration Test
- File: `Fair/AuthorExpiration.cs`
- Description:  
```
This tests verifies expected effects of author expiration.
```
---
### Author Info Change Test
- File: `Fair/AuthorInfo.cs`
---
### Account List Authors Command Test
- File: `Cmd/AccountListAuthors.cs`
- Description:  
```
This verifies process of listing user's authors.
Authors created for a user, and the 'user la' output entry count is verified.
```
---
### Author Avatar Change Test
- File: `Cmd/AuthorAvatar.cs`
---
### Author Create Command Test
- File: `Cmd/AuthorCreation.cs`
- Description:  
```
Author creation tests: <br/>
- Not sufficient funds. Fail expected. <br/>
- Minimal time: 1 year. <br/>
- Max time: 10 years. <br/>
- Exceeding max time of 10 years. Exception expected. <br/>
- With already existing title. <br/>
```
---
### Author Entity Query Test
- File: `Cmd/AuthorDomainVerification.cs`
- Description:  
```
```
---
### Author Entity Query Test
- File: `Cmd/AuthorEntity.cs`
- Description:  
```
Author entity command test.
- Base case - checking entity of an existing author.
- Non-existing id.
```
---
### Author Nickname Change Test
- File: `Cmd/AuthorName.cs`
---
### Author Renewal Test
- File: `Cmd/AuthorRenewal.cs`
- Description:  
```
- Smoke test. <br/>
- Not sufficient funds attempt. Expected to fail. <br/>
- Renewal for more than 10 years in total. Failure expected. <br/>
- Renewal for maximal 10 years in total.
```
---
### Author Security Command Test
- File: `Cmd/AuthorSecurity.cs`
- Description:  
```
```
---
### Fair User Creation Test
- File: `Cmd/FairUserCreation.cs`
- Description:  
```
Check that `account create` command works as expected. <br/>
- Basic smoke testing; <br/>
- Creation of an account for an account address that already has an account; <br/>
- Bad `owner` account address string parameter; <br/>
- Creation of an account for an account address that does not exist; <br/>
Each command line parameter is input tested. <br/>
```
---
### User Entity Test
- File: `Cmd/FairUserEntity.cs`
- Description:  
```
Check that `account entity` command works as expected. Basic smoke testing;
Attempt with non-existant acc address; Each command line parameter is input tested.
```
---
### Fair User Utility Transfer Test
- File: `Cmd/FairUserUtilityTransfer.cs`
- Description:  
```
Check that `utility transaction` command works as expected. Basic smoke testing;
Not enough funds test; Non-existent receiver entity id test; Non-existent sender entity id test;
Bad signer test; Each command line parameter is input tested.
```
---
### File Creation Test
- File: `Cmd/FileCreation.cs`
---
### File Destroy Test
- File: `Cmd/FileDestroy.cs`
---
### File Entity Test
- File: `Cmd/FileEntity.cs`
---
### Product Creation Test
- File: `Cmd/ProductCreation.cs`
- Description:  
```
- Not sufficient funds attempt, failure expected. <br/>
- Smoke test. <br/>
- Attempt to creat a product for a foreign author (author that is not registered on a signer account address). Failure expected. <br/>
```
---
### Product Entity Command Test
- File: `Cmd/ProductEntity.cs`
- Description:  
```
- Smoke test <br/>
- Non-existent product id, exception expected.
```
---
### Site Creation Test
- File: `Cmd/StoreCreation.cs`
- Description:  
```
Tests site creation functionality including specific cases: <br/>
- Not sufficient funds fail. <br/>
- Creation for minimal time - 1 year. <br/>
- Creation for maximal time - 10 years. <br/>
- Creation for a long time - 200 years. Fail expected. <br/>
- Creating a site with a title that already exists. This is allowed. <br/>
<br/>
Cmd arguments are individually tested for junk input. <br/>
<br/>
<b>TODO</b>: Site lifetime expiration is not implemented yet! Releases of any kind should not be 
allowed until then. Tests validating lifetime expiration should be implemented as soon as
implementation takes place.
```
---
### Site Entity Command Test
- File: `Cmd/StoreEntity.cs`
- Description:  
```
- Smoke test. <br/> 
- Case with non-existent site id, exception expected. <br/> 
- Command params input-tested.
```
---
### Site Renewal Test
- File: `Cmd/StoreRenewal.cs`
- Description:  
```
- Smoke test. <br/>
- Not sufficient funds. Fail expected. <br/>
- Attempt to renew for longer then 10 years in total. Fail expected. <br/>
- Renewal for exactly 10 years in total.<br/>
- Command params input-testing.
```
---
### Discussed Proposal Close Fail Test
- File: `Fair/DiscussedProposalCloseFail.cs`
- Description:  
```
This test simulates a site description dispute, where 6 participants have a conversation using dispute comments. 
They comment and vote, change their comments. 2 positive votes are cast, and 3 negative after.
It is expected for the vote option that reaches half of the votes first to be accepted (this way tie is impossible). Note: this should be the case only 
for proposals with even voters, when the number of voters is uneven - ties are impossible, hence \"true\" majority of votes is expected.
```
---
### Solo Creator End To End Test
- File: `E2E/SoloCreator.cs`
- Description:  
```
This e2e test represent a use case when a user creates account, site, site category, product, and then publishes the product 
on just created site, all by his own.
```
---
### Fee Validation Test
- File: `Fair/FeeValidation.cs`
- Description:  
```
Operation space/time fee validation tests.
- Site creation, 1 year.
- Site creation, 7 years.
- Site top-up.
- Author creation, 1 year.
- Author creation, 6 years.
- Author top-up.
- Product creation.
- Product update: setting a title to a single character.
- Product update: setting a title to a 2 character string.
- Product update: setting a title back to a 1 character string.
- Proposal to publish a product.
- Site user registration.
- Site category creation.
- File creation.
- File deletion.
```
---
### Misc Test
- File: `Fair/MiscTest.cs`
- Description:  
```
```
---
### Publication Removal Test
- File: `Fair/PublicationRemoval.cs`
- Description:  
```
Site with a publisher and a publication is created. The publication is then deleted. Each publication lifetime state is verified.
```
---
### Review Creation Test
- File: `Fair/ReviewCreationAndEdit.cs`
- Description:  
```
- Review creation, its existence and content verified.
- Review edited, its existence and new content verified.
```
---
### Site Category Creations Test
- File: `Fair/StoreCategoryCreation.cs`
- Description:  
```
This test simulates the cases of new site categories creation. First, a site with 4 published authors is created. Few moderators are added. 
Then, new category creation dispute is created, voted for, results validated. Another category, same process. Sub-category creation, validated. Sub-sub-category creation, validated. 
Each actor uses separate OS.
```
---
### Store Expiration Test
- File: `Fair/StoreExpiration.cs`
- Description:  
```
This tests verifies expected effects of store expiration.
According to @Maximion The Great, effects are:
- Nothing gets deleted.
- No store operations can be executed. Loss of control.
- "Names can be taken" (TODO @krs Not sure about this one, need to check.)
```
---
### Site Moderators Test
- File: `Fair/StoreModerators.cs`
- Description:  
```
This test simulates the e2e case of adding new moderators to the site. First, a site with 4 published authors is created. 
Then, moderation addition proposals are created and voted for. Each actor uses separate OS.
```
---
### Site Policy Change Test
- File: `Fair/StorePolicyChange.cs`
- Description:  
```
This test simulates the case of changing policies of the site. First, a site with 4 published authors is created. 
Then, policy change dispute is created and voted for. Voting results are validated. Change to each policy verified. Each actor uses separate UOS.
```
---
### Site Text Change Test
- File: `Fair/StoreTextChange.cs`
- Description:  
```
This test simulates the cases of changing site text properties: description, slogan and title. Multiple changes, including a change under a different policy. 
Description changes verified. All actors represented by unique OS-es.
```
---
### Site Voting Test
- File: `Fair/StoreVoting.cs`
- Description:  
```
This test checks the process of creating a nickname change proposal, and its approval. 
3 authors 2/3 majority case; with new authors case; case with negative votes; Fail case. 
Nickname change dynamic verified after each case.
```
---
### Unanimous Proposal Fail Test
- File: `Fair/UnanimousProposalFail.cs`
- Description:  
```
This test is focused on a logic of unanimous voting fails. It is expected for unanimous proposal to 
fail after a single negative vote. Unanimous votes succeed only when all votes are positive, that's why even a single 
negative vote is enough to fail and cancel such voting.
```
---
### Variadic Proposal Test
- File: `Fair/VariadicProposal.cs`
- Description:  
```
This test checks the process of creating a proposal with multiple options, and different cases of voting for it. 
Results are verified.
```
---
### Domain Acquire Command Test
- File: `Cmd/DomainAcquireCmd.cs`
- Description:  
```
Check that domain acquisition with `domain acquire` command works as expected.
Checks for cases when domain is already acquired by other account; When no domain name provided;
Each command parameter is input tested.
```
---
### Domain Entity Query Test
- File: `Cmd/DomainEntityCmd.cs`
- Description:  
```
This test checks that 'domain entity' command works as expected.
- Basic smoke testing;
- Case with nonexistent domain, exception expected;
Command params input tested.
```
---
### Domain Migrate Command Test
- File: `Cmd/DomainNameMigrateCmd.cs`
- Description:  
```
This tests migration of a web domain to the Ultranet with a `domain migrate` command.
Migration represents a process of acquiring a domain name on the basis of owning a web domain in the traditional web.
User has to locate his Ultranet account address in the DNS records of a web domain to be able to migrate.
- Basic smoke testing; 
- Non-existing domain test; 
Command parameters are input tested.
```
---
### Domain Policy Update Test
- File: `Cmd/DomainPolicyUpdate.cs`
- Description:  
```
This tests sub-domain policy update command.
- Generic case, smoke testing.
- Change from FullOwnership to FullFreedom.
- Attempts to change policy of a subdomain under FullFreedom, failure expected.
- Attempting to update policy of a nonexistent subdomain, exception expected.
- Attempt to update policy of a subdomain by nonexistent username, exception expected.
Command parameters fuzzy tested.
```
---
### Domain Renew Test
- File: `Cmd/DomainRenewCmd.cs`
- Description:  
```
Tests `domain renew` command - prolongation of a domain name rent. Basic smoke testing;
Command parameter input testing.
```
---
### Domain Security Command Test
- File: `Cmd/DomainSecurity.cs`
- Description:  
```
```
---
### Domain Transfer Command Test
- File: `Cmd/DomainTransferCmd.cs`
- Description:  
```
Tests `domain transfer` command - transfer of a domain from one user to another.
Basic smoke testing; Attempt to transfer non-existent domain; Command parameters input test.
```
---
### Economy Cost Command Test
- File: `Cmd/EconomyCostCmd.cs`
- Description:  
```
Tests `economy cost` command - network fee information query. Command parameters input tested.
```
---
### Link Creation Test
- File: `Cmd/LinkCreateCmd.cs`
- Description:  
```
This tests a resource-to-resource link creation with 'link create` command. Basic smoke testing;
Command parameters input tested.
```
---
### Link Destroy Command Test
- File: `Cmd/LinkDeletion.cs`
- Description:  
```
This tests resource-to-resource link deletion with `link delete` command. Basic smoke testing;
Attempt to delete non-existent link; Command parameters input testing.
```
---
### Link List Inbounds Test
- File: `Cmd/LinkListInboundsCmd.cs`
- Description:  
```
Tests command `link li` - list all links that point to this resource. Basic smoke testing;
Links count verification; Listing for a non-existent resource; Command parameters input testing.
```
---
### Link List Outbounds Test
- File: `Cmd/LinkListOutboundsCmd.cs`
- Description:  
```
Tests command to list all outgoing links or a resource - `link lo`. Basics smoke test;
Links count verification; Link listing for non-existent resource; Command parameters input testing.
```
---
### Rdn User Creation Test
- File: `Cmd/RdnUserCreation.cs`
- Description:  
```
- Smoke test. Basic correct user creation case.
- Not sufficient funds to create a user, failure expected.
- Top-up user and try again, expected to work. 
- Creation of user with already existing username and owner address. Failure expected.
- Creation of user with existing username but new owner address. Failure expected.
- Too short/ long username tests. Exceptions expected.
- Command-line parameters fuzz-tested.
```
---
### User Entity Test
- File: `Cmd/RdnUserEntity.cs`
- Description:  
```
Check that `user entity` command works.
- Basic smoke testing, asserted that console output contains owner's account address;
- Attempt with nonexistent user name, exception expected;
Each command line parameter is fuzzy tested.
```
---
### Rdn User Utility Transfer Test
- File: `Cmd/RdnUserUtilityTransfer.cs`
- Description:  
```
Check that `utility transaction` command works as expected.
- Basic smoke testing: successful utility transaction;
- Not enough funds for a transfer, failure expected;
- Nonexistent receiver entity id, failure expected;
- Nonexistent sender entity id, failure expected;
- Bad signer test, exception expected;
Each command line parameter is input tested.
```
---
### Release Creation Test
- File: `Cmd/ReleaseCreateCmd.cs`
- Description:  
```
Tests release creation command `release create`. 
- Basic smoke test.
- Attempting creation of a release from the file already in use by existing release, exception expected.
- Attempt to create release from nonexistent file, exception expected.
Command parameters fuzzy tested.
```
---
### Release Local Command Test
- File: `Cmd/ReleaseLocalCmd.cs`
- Description:  
```
Tests release local copy information query command `release local`. Basic smoke test;
Checking availability for non-existent release; Command parameters input testing.
```
---
### Resource Creation Command Test
- File: `Cmd/ResourceCreation.cs`
- Description:  
```
Tests resource creation command `resource create` by creating a 'raw' (of data type `Data`) resource 
and a counsil resource. Checks 'Status' output field; Command parameters are input tested.
```
---
### Resource Destroy Command Test
- File: `Cmd/ResourceDestroy.cs`
- Description:  
```
Tests command to destroy a resource `resource destroy`. Basic smoke test, attempt to destroy non-existant just destroyed resource;
Command parameters input tested.
```
---
### Resource Download Cancellation Cmd Test
- File: `Cmd/ResourceDownloadCancellationCmd.cs`
- Description:  
```
This tests command `resource dx` that supposed to cancel ongoing resource downloading.
This test downloads a 100mb file and executes the command while its happening. This is not a consistent 
way to test, discussion: 
https://www.notion.so/Test-resource-download-cancellation-with-resource-dx-1cb4e2aecdb980a2bf65d8ae72e4d1d2?pvs=4 ;
Command parameters are input tested.
```
---
### Resource Entity Command Test
- File: `Cmd/ResourceEntityCmd.cs`
- Description:  
```
Tests resource information query command `resource entity`. Simple smoke test; Command parameter input testing.
```
---
### Resource Local Command Test
- File: `Cmd/ResourceLocalCmd.cs`
- Description:  
```
Tests query about locally available releases of a specified resource `resource local`.
Simple smoke test; Command parameters input tested.
```
---
### Resource Local Search Command Test
- File: `Cmd/ResourceLocalSearchCmd.cs`
- Description:  
```
Tests locally available resource search `resource localsearch`.
Simple smoke test, checking that correct amount of results returned, checking 
that they are valid and contain the search substring. Checking that resource is 
not returned after associated files are deleted.
```
---
### Resource Update Command Test
- File: `Cmd/ResourceUpdateCmd.cs`
- Description:  
```
Tests command `resource update`. Basic smoke test of updating an existing `Data` resource with new data.
```
---
### Sub Domain Creation Command Test
- File: `Cmd/SubDomainCreation.cs`
- Description:  
```
Tests subdomain creation command `domain cs`. Basic smoke testing; Nested sub-domains; Command parameters input tested.
```
---
### Bandwidth Allocate Command Test
- File: `Cmd/UserBandwidthAllocation.cs`
- Description:  
```
Check that `bandwidth allocate` command works as expected.
- Basic smoke testing (default use case);
- Not sufficient funds for allocation attempt, fail expected;
Each command line parameter is input tested.
```
---
### Analysis Publication E2E Test
- File: `E2E/AnalysisPublicationE2E7.cs`
- Description:  
```
This e2e test represents a use case when user A creates a file resource, and creates 'analysis request'.
Resource is then being analyzed by the earlier created consil members, analysis results are uploaded.
Consil/analysis resource creation and analysis update transaction integrity is checked.
```
---
### Directory Sharing E2E Test
- File: `E2E/DirectorySharingE2E2.cs`
- Description:  
```
This e2e test represents a use case when user A shares a folder with user B using Ultranet. User A 
acquires a domain and locates a directory-resource there. User B then, downloads a resource and reads folder 
contents, verifying them.
```
---
### Domain Acquisition Conflict E2E Test
- File: `E2E/DomainAcquisitionConflictE2E3.cs`
- Description:  
```
This e2e tests represents a use case when user B attempts to acquire domain that 
is already acquired by user A. User B is expected to get failedornotfound status on his request.
```
---
### Domain Expiration E2E Test
- File: `E2E/DomainExpirationE2E4.cs`
- Description:  
```
This e2e test represents a usecase when user attempts to use the domain 
after its expiration. User acquires a domain for a year, more then a year passes, 
user attempts to create resource or update sub-domain policy, and is expected to fail to do so.
```
---
### File Sharing E2E Test
- File: `E2E/FileSharingE2E1.cs`
- Description:  
```
This e2e test represents a use case of two users sharing a file using Ultranet.
User A creates a file, acquires a domain, locates resource containing a file there. Then 
user B downloads a file-containing resource and verifies its contents.
```
---
### Analysis Publication By Non-Affiliated E2E Test
- File: `E2E/OutsiderAnalysisPublicationE2E.cs`
- Description:  
```
This e2e test represents a use case when attempt to publish a analysis result is being made by a user
that is not a part of counsil.
```
---
### Wrong Data Type Resource Creation Testset
- File: `Rdn/InvalidResourceDownload.cs`
- Description:  
```
Tests attempt to download an ivalid resource, one that was created with a bad Rrrh.
```
---
### Packages Testset
- File: `Rdn/PackagesTest.cs`
- Description:  
```
```
---
### Dependencies Test Set
- File: `Testsets/Dependencies.cs`
---
### Impersonation Testset
- File: `Testsets/ImpersonationTestset.cs`
- Description:  
```
This testset is a group of attempts to perform some kind of action 
on behalf of another user. Such actions should never go through and get executed.
There is an attempt to acquire domain using foreign signer account address.
Attempt to steal resources fron an account, signing with victim's account address 
and thief signing himself.
```
---
### Transaction Integrity Testset
- File: `Testsets/TransactionIntegrityTestset.cs`
- Description:  
```
This testset is aimed to test correctness of transactions and operation fee calculations.
Ensuring that exact amount is deducted and received during transactions or other operations. TODO: More details!
This testset is huge and needs more explanation.
```
---
### Wrong Data Type Resource Creation Testset
- File: `Testsets/WrongResourceDataTypeTestset.cs`
- Description:  
```
This set of tests checks cases when resource is being created with a wrong data type provided.
```
---
### Wallet Add Account Command Test
- File: `Tests/WalletAccountAdd.cs`
- Description:  
```
Tests command to add a new account to a wallet. Verifies that wallet object contains +1 account after execution.
```
---
### Wallet Creation
- File: `Tests/WalletCreation.cs`
- Description:  
```
Tests wallet creation process via `wallet create` command . 
Smoke test + wallet file creation validation + console output validation; 
Cmd fuzzy test.
```
---
### Wallet Import Test
- File: `Tests/WalletImport.cs`
- Description:  
```
Tests uos wallet file import with `wallet import` command. Simple smoke test; 
Command parameters input tested.
```
---
### Wallet Import Account By Key
- File: `Tests/WalletImportAccountByKey.cs`
- Description:  
```
Tests importing of an existing account to a wallet using an account private key with `wallet addaccount` command.
- Key import into a default wallet ('name' parameter is not specified).
- Default case. Smoke test.
- Importing account into a wallet that does not exist. Exception expected.
- Importing a bad key. Exception expected.
Command parameters fuzzy tested.
```
---
### Wallet Locking
- File: `Tests/WalletLock.cs`
- Description:  
```
Tests wallet locking with `wallet lock` command. Simple smoke test; Command parameters input tested.
```
---
### Wallet Unlock Test
- File: `Tests/WalletUnlock.cs`
- Description:  
```
Tests `wallet unlock` command that allows to use underlying account keys.
- Attempt to add an account to the locked wallet. Exception expected.
- Attempt to unlock a wallet with wrong password. Exception expected.
- Default smoke test.
- Attempt to unlock a wallet that does not exist. Exception expected.
Command parameters are input tested.
```
---
