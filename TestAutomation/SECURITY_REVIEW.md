# Ultranet UC — Security Review

**Reviewed revision:** `d26c61700` (branch `claude/code-security-review-2c2752`), 23 November 2026

**Type of review:** manual source-code reading. No code was changed, built, run or tested against a live network, and no finding below has been exercised with a working exploit. This file is the only thing that was added.

---

## 1. How to read this report

**Scope.** `CS/`, `Net/`, `Nexus/` (including the Rdn chain), `Fair/cs`, `Fair/WebUI`, `Web/`, the hosts (`Nexus.CLI`, `Nexus.Windows`, `Rdn.CLI`, `Fair.CLI`, `Fair.Windows`, `Mcv.FUI`), `TestAutomation/`, `Tests/`, `Umc/`, `Demo/`, `Nas.sol`, and repository hygiene. The native C++ side (`Uos/`, `Shell/`, `Vwm/`, `Experimental/`, `C/`, `Image.DevIL/`, `Libs/`) was not reviewed.

**Severity.**

| Level | Meaning |
|---|---|
| **Critical** | Loss of keys or funds, code execution on a user's machine, or a halt/takeover of the network, reachable with little effort and no special role. |
| **High** | Serious impact that needs a precondition (a consensus member, a user click, local access, a syncing node), or a reliable crash of single nodes. |
| **Medium** | Limited impact, harder preconditions, or a missing safeguard that makes other issues worse. |
| **Low** | Hardening, low-probability problems, minor leaks. |
| **Info** | Observations: unfinished code, hygiene, things to decide. |

**Confidence.** Each finding is marked:

- **Verified** — I followed the code path from the untrusted input to the effect, including the checks in between.
- **Likely** — the defect is in the code, but part of the path or the real-world effect was not fully traced.
- **To confirm** — depends on behaviour I could not establish from the code alone (deployment, browser behaviour, intended design).

**Suggested direction** lines are deliberately short; they point at the kind of fix, not a design.

---

## 2. Summary

**114 findings:** 13 Critical, 32 High, 44 Medium, 19 Low, 6 Info. Several entries group related smaller points, so the number of individual observations is higher.

The platform is clearly still being built, and the pattern across the findings reflects that: the main mechanisms exist and mostly work for cooperative participants, but the parts that face untrusted input — peer messages, consensus votes, the local APIs on users' machines, downloaded releases — largely assume the other side behaves. Many findings are a single missing check; a few are design gaps that need a decision.

### What matters most

1. **The local APIs trust every caller** (A1–A7, F1). Any program on the machine — and, as configured, any web page — can have unlocked keys sign anything without a prompt, make the Nexus download and run software, and read or write the user's files. Where a prompt is shown, declining it does not stop the signature (A3).
2. **Consensus acts on messages before checking them** (B2–B9). A single small unauthenticated message can stop member nodes or get an honest member flagged as a cheater. Because any unexpected exception stops the whole node (B5), ordinary coding slips become remote off-switches.
3. **Value can be created from nothing** (B1, E1, G2). There are four independent ways to mint energy or spacetime; operations are in practice free (B10) and consensus seats are cheap (B11).
4. **Nodes believe single peers** (C1, C3, C8, F4). Synchronisation and light-client lookups have no fixed point of trust, and anyone can rewrite a node's peer list.
5. **Downloaded content is not contained** (F2–F6, A4). File names from the network are used as paths, and packages run without any trust decision.
6. **Store governance on Fair can be forged** (G1). Anyone can cast a moderator's vote.

### A possible order of work

Nothing has been changed; this is only a suggestion for planning.

1. A1, A2, A3 — a few lines each — then put real access control on the local APIs (A4–A7, F1).
2. B1, B3, B4, B2, then contain exceptions (B5) and validate what votes carry (B7).
3. G1, G2, E1 — the authorisation and value checks in chain operations.
4. C2 and C4 (cheap remote denial of service), then C3 and C1.
5. F2 and F3 (keep file paths inside their folders).
6. J1 — replace the committed signing key.

### All findings by severity

| # | Severity | Finding |
|---|---|---|
| A1 | Critical | `Batch` call skips the admin-password check |
| A2 | Critical | `OverrideAuthentication` turns off the approval dialog |
| A3 | Critical | Rejecting a signing request still signs |
| A4 | Critical | Nexus `Do` call (and `iccp:` links) download and run any package |
| B1 | Critical | Energy can be created out of nothing, in two ways |
| B2 | Critical | A member can be accused of double-voting by anyone, without a signature |
| B3 | Critical | Transactions with an empty signature are treated as signed |
| B4 | Critical | A vote with an empty body stops member nodes |
| E1 | Critical | Analysis rewards can create energy and spacetime |
| F1 | Critical | Resource and package API calls read and write arbitrary local files |
| F2 | Critical | Downloading a directory release can write files anywhere |
| G1 | Critical | Anyone can cast a moderator's vote |
| G2 | Critical | Energy can be drained from a store, or created, through rewards and fees |
| A5 | High | Any web site can call the local APIs |
| A6 | High | Node and Nexus APIs have no protected calls; `Transact` signs with the node's stored session |
| A7 | High | "Property" calls read arbitrary internal objects |
| A8 | High | The vault signs opaque hashes; what the user is told is supplied by the caller |
| A9 | High | The application asking for access names itself |
| A10 | High | Importing a wallet can overwrite any wallet file |
| B5 | High | Any unexpected exception shuts the node down |
| B6 | High | Unchecked votes are stored and relayed |
| B7 | High | Transactions inside votes are never validated |
| B8 | High | Accounts can be created without a key, which later crashes every node |
| B9 | High | Public keys are not validated |
| B10 | High | Operations are effectively free |
| B11 | High | Consensus seats are cheap, and one bad declaration blocks all clients |
| B12 | High | Off-chain check results are kept forever and resent in every vote |
| C1 | High | A synchronising node believes whatever one peer tells it |
| C2 | High | Lengths from the wire are used to allocate memory |
| C3 | High | Anyone can rewrite the peer list |
| C4 | High | A half-closed connection spins a thread forever |
| C5 | High | Synchronisation dies on ordinary errors |
| D1 | High | The Nexus accepts ICCP from the whole network and relays it to local nodes |
| E2 | High | Reporting an analysis result changes shared data in place |
| E3 | High | Domain names and domains are stored in the same database tables |
| F3 | High | Package deployment extracts and deletes outside its folder |
| F4 | High | Nothing decides whether a package may run |
| F5 | High | Releases are marked complete before their files are |
| F6 | High | Downloaded data is trusted until the very end |
| F9 | High | Seed discovery can stop the node |
| G3 | High | Moving a category is not checked against loops or ownership |
| G4 | High | Stored proposals behave differently after a restart |
| H1 | High | A category that is its own parent takes the node down with one request |
| H2 | High | The web API reads chain state without the chain lock |
| J1 | High | Android signing keystore and its passwords are in the repository |
| A11 | Medium | Wallet encryption has no integrity check |
| A12 | Medium | Password hashing can crash or stall the vault |
| A13 | Medium | Admin password handling |
| A14 | Medium | TLS certificates are not checked |
| A15 | Medium | Secrets at rest and on the command line |
| B13 | Medium | Quorum is `n × 2 / 3` rounded down |
| B14 | Medium | Votes claiming the genesis generator skip verification |
| B15 | Medium | Transaction pool eviction |
| B16 | Medium | Transaction status queries can be broken for everyone |
| B17 | Medium | Consensus time is not tied to local clocks |
| C6 | Medium | Connection limits |
| C7 | Medium | No encryption or peer authentication |
| C8 | Medium | Unsynchronised clients trust single answers |
| D2 | Medium (must not ship as is) | Inter-chain transfers are unfinished and unauthenticated |
| D3 | Medium | Local pipe between Nexus and nodes |
| E4 | Medium | Reserved names are protected only by a local file check |
| E5 | Medium | Resource links |
| E6 | Medium | Renaming a domain leaves stale records |
| F7 | Medium | Seed hubs accept any declaration and evict honest seeders |
| F8 | Medium | File-serving requests are not bounded |
| F10 | Medium | Dependency cycles crash the daemon |
| F11 | Medium | Parsers trust depth and counts |
| F12 | Medium | A dependency's pinned hash is not used |
| G5 | Medium | A failed limit check is overwritten |
| G6 | Medium | "Majority" is half |
| G7 | Medium | File type and size rules |
| G8 | Medium | Product fields can reference files that do not exist or belong to others |
| G9 | Medium | Author rank is read from a local file while a round is being confirmed |
| G10 | Medium | Review storage is booked to the author but released from the store |
| G11 | Medium | More operations that throw instead of failing |
| H3 | Medium | Search does unbounded work per request |
| H4 | Medium | No request limits on a server that shares a process with consensus |
| H5 | Medium | Whole collections are loaded to return one page or a count |
| H6 | Medium | One stale or unusual record turns a whole page into an error |
| H7 | Medium | Errors raised while the response is written bypass the error handling |
| I1 | Medium | Ids from the address bar go straight into API paths, and the answer decides what is signed |
| I2 | Medium | The VK video embed frames any site |
| I3 | Medium | The addresses of the local vault and node come from the remote API |
| J2 | Medium | `iccp:` links start a second Nexus with a chosen profile and zone |
| J3 | Medium | Sessions cannot be revoked and never expire |
| J4 | Medium | Approval dialogs |
| J5 | Medium | One genesis ("Father") key for every zone |
| J8 | Medium | Runtime and dependencies |
| J9 | Medium / Info (currently unused) | `Nas.sol` |
| A16 | Low | Error details, request size, test calls |
| A17 | Low | Secrets stay in memory |
| B18 | Low / Info | Smaller observations |
| C9 | Low | Request handling |
| D4 | Low | ICCP packet reading |
| E7 | Low / Info | Smaller observations |
| F13 | Low | Privacy of local releases |
| F14 | Low / Info | Smaller observations |
| G12 | Low / Info | Smaller observations |
| H8 | Low | Input validation gaps |
| H9 | Low | Response and host hardening |
| I4 | Low | Links stored on chain are loaded as images and media |
| I5 | Low | Sessions in the browser |
| I6 | Low / Info | Smaller observations |
| J6 | Low | Wallet screens |
| J7 | Low | Unlock prompt |
| J10 | Low | Debug and Release builds speak different formats |
| J11 | Low | Test tooling |
| J12 | Low / Info | Repository hygiene |
| C10 | Info | State larger than one packet cannot be transferred |
| H10 | Info | Moderation and user endpoints are public |
| H11 | Info | Smaller observations |
| J13 | Info | Build settings |
| J14 | Info | Mobile app (`Umc`) |
| J15 | Info | Vote hash uses one byte of the zone number |

---

## 3. Findings

### A. Local HTTP APIs, Vault and the Nexus daemon

These run on every user's machine. The vault holds the private keys; the Nexus and node APIs can start transactions and software. By design the public web UI talks to them from the browser, so "who is allowed to call them" is the central question — and at the moment the answer is close to "anyone".

#### A1. `Batch` call skips the admin-password check — **Critical** · Verified
**Where:** [JsonServer.cs:190](../CS/JsonServer.cs#L190), [JsonServer.cs:298](../CS/JsonServer.cs#L298), restricted list in [VaultApi.cs:14](../Nexus/VaultApi.cs#L14)
**What:** The server checks the "restricted" list only against the call name in the URL. A `Batch` request is not restricted, and its inner calls are created and executed with no check at all.
**Abuse:** Anyone who can reach the vault API wraps `ImportWallet`, `Wallets`, `WalletKeys`, `UnlockWallet`, `LockWallet`, `AddKeyToWallet` or `OverrideAuthentication` in a `Batch` and it runs without the admin password. It works even when no admin password has been set.
**Direction:** Apply the restriction check to every call that is executed, wherever it comes from; consider removing `Batch` from servers that have restricted calls.

#### A2. `OverrideAuthentication` turns off the approval dialog — **Critical** · Verified
**Where:** [VaultApi.cs:174](../Nexus/VaultApi.cs#L174)
**What:** This production API call replaces the "an application wants to authenticate" dialog with a handler that always answers "allow, always" for the key whose alias equals the requested user. It is protected only by the admin-password list, which A1 bypasses.
**Abuse:** `Batch{OverrideAuthentication}` → `Authenticate` → receive a session with "always allow" → `Authorize` any hash. Every key in an unlocked wallet can then sign anything, with nothing shown to the user. `WalletKeys` (also via `Batch`) lists the aliases needed.
**Direction:** Remove from production builds, or allow it only in test zones and never over the network API.

#### A3. Rejecting a signing request still signs — **Critical** · Verified
**Where:** [Vault.cs:237](../Nexus/Vault.cs#L237), handler in [NexusWindows.cs:151](../Nexus.Windows/NexusWindows.cs#L151)
**What:** `Authorize` calls the approval handler and throws its answer away: `AuthorizationRequested(acc.Public, au, operation);` then signs. The Windows handler returns `false` on Cancel, which nobody reads.
**Abuse:** "Ask every time" and the "manual approval required" setting for transfers and key changes give no protection: the signature is returned whatever the user clicks. (On hosts with no handler the call throws instead, so they fail closed by accident.)
**Direction:** Refuse unless the handler returns true; treat a missing handler as refusal.

#### A4. Nexus `Do` call (and `iccp:` links) download and run any package — **Critical** · Verified
**Where:** [NexusApi.cs:146](../Nexus/NexusApi.cs#L146), [Nexus.cs:177](../Nexus/Nexus.cs#L177), [Nexus.cs:244](../Nexus/Nexus.cs#L244), [Nexus.cs:253](../Nexus/Nexus.cs#L253), protocol handler [NexusWindows.cs:247](../Nexus.Windows/NexusWindows.cs#L247), [OpenCommand.cs:25](../Nexus/CLI/OpenCommand.cs#L25)
**What:** `Do` takes an address, resolves it on the Rdn network, deploys the package and starts it with `Process.Start` (`UseShellExecute = true`, path and arguments taken from the package's own instruction). The call needs no password and no user confirmation. For a net that is not running locally, the node software to install is taken from whatever answers the `Info` query for that net.
**Abuse:** A local process — or a web page, see A5 — sends `Do` with the address of a package the attacker published; it is downloaded and executed as the user. The registered `iccp:` URL scheme leads to the same call, so a link on any page needs only the browser's "open application?" click.
**Direction:** Require explicit user consent per package/publisher before first run, authenticate the caller, and never auto-install node software named by an unauthenticated network answer.

#### A5. Any web site can call the local APIs — **High** · Verified in code; browser behaviour to confirm
**Where:** [JsonServer.cs:181](../CS/JsonServer.cs#L181), [JsonServer.cs:216](../CS/JsonServer.cs#L216), [JsonServer.cs:285](../CS/JsonServer.cs#L285)
**What:** Every API (vault, Nexus, Rdn node, Fair node) answers with `Access-Control-Allow-Origin: *`, never looks at the `Origin` header or the HTTP method, and parses the body as JSON whatever its content type. A plain cross-site `POST` therefore executes and the page can read the answer. A request with no body is executed with default arguments, so calls that need no parameters — `Exit`, or `Authenticate`, which opens a dialog — can be triggered by nothing more than an image tag.
**Abuse:** This is what turns A1–A4, A6 and A7 from "local attacker" into "a page the user happens to open". Newer browsers ask for permission before a public site may reach loopback addresses, but the legitimate UI needs that same permission, so users will be used to granting it.
**Direction:** Allow-list origins, require a per-installation secret or pairing step, and reject requests whose content type is not JSON.

#### A6. Node and Nexus APIs have no protected calls; `Transact` signs with the node's stored session — **High** · Verified
**Where:** [McvApi.cs:279](../Net/Mcv/McvApi.cs#L279), [McvPeering.cs:1149](../Net/Mcv/McvPeering.cs#L1149), [NodeApi.cs:68](../Net/Node/NodeApi.cs#L68), [McvApi.cs:396](../Net/Mcv/McvApi.cs#L396)
**What:** Only the vault server registers restricted calls. On the node APIs, `Transact` accepts a user name and operations; when no session is passed it uses the session the node has stored for that user. `Exit` stops the node. `Ppc` relays arbitrary peer requests. `LogReport`, `IncomingTransactions`, `OutgoingTransactions` return logs and transactions.
**Abuse:** Any caller can submit operations as any user the node holds a session for (node operators, CLI users), stop the node, or read its logs. With A3 the vault's approval step does not stop the transaction.
**Direction:** Authenticate callers of every state-changing or sensitive call; never fall back to a stored session for an unauthenticated request.

#### A7. "Property" calls read arbitrary internal objects — **High** · Verified
**Where:** [McvApi.cs:84](../Net/Mcv/McvApi.cs#L84), [NodeApi.cs:31](../Net/Node/NodeApi.cs#L31), [NexusApi.cs:119](../Nexus/NexusApi.cs#L119)
**What:** `McvProperty`, `Property` and `NexusProperty` walk a caller-supplied dotted path over public properties and fields by reflection. `McvProperty` returns the object itself, which is then serialized as JSON.
**Abuse:** `McvProperty{Path:"Settings"}` returns the node settings including the stored vault session tokens ([McvNodeSettings.cs:23](../Net/Mcv/McvNodeSettings.cs#L23)) and the admin-password hash. `NexusProperty{Path:"Vault.PasswordAsker.Password"}` returns the wallet password last typed on a console host ([ConsolePasswordAsker.cs:56](../CS/ConsolePasswordAsker.cs#L56)). A stolen session token is all `Authorize` needs.
**Direction:** Remove, or replace with a fixed list of named, harmless values.

#### A8. The vault signs opaque hashes; what the user is told is supplied by the caller — **High** · Verified
**Where:** [Vault.cs:208](../Nexus/Vault.cs#L208), [VaultApi.cs (Net)](../Net/VaultApi.cs), caller side [McvPeering.cs:927](../Net/Mcv/McvPeering.cs#L927)
**What:** `Authorize` receives a ready-made hash plus a free-text `Operation` description, a `ManualApprovalRequired` flag and the signature scheme — all chosen by the caller. The vault cannot tell what the hash stands for. The session's `Net` is only a lookup key; it does not restrict what is signed. Votes and transactions are signed through the same call with no separating prefix.
**Abuse:** An application holding a session for one net can obtain signatures valid on another net where the same key is used, can describe a transfer as something harmless, and can simply say that manual approval is not required.
**Direction:** Send the transaction itself to the vault, let the vault build the hash and the description, decide about prompts inside the vault, and bind sessions to net and application.

#### A9. The application asking for access names itself — **High** · Verified
**Where:** [Vault.cs:186](../Nexus/Vault.cs#L186), [NexusWindows.cs:134](../Nexus.Windows/NexusWindows.cs#L134)
**What:** The application name and logo shown in the authentication dialog come from the request. The vault does not record or show where the request came from, does not limit how often it can be asked, and offers "always allow".
**Abuse:** Any page can present itself as the Fair store with the real logo, and repeat the prompt until the user gives in. One approval with "always allow" gives silent signing rights (see A8).
**Direction:** Show and bind the real origin of the request, rate-limit prompts, and make "always allow" exclude transfers and key changes.

#### A10. Importing a wallet can overwrite any wallet file — **High** · Verified
**Where:** [Vault.cs:134](../Nexus/Vault.cs#L134), [Wallet.cs:161](../Nexus/Wallet.cs#L161), [VaultApi.cs:42](../Nexus/VaultApi.cs#L42)
**What:** `CreateWallet` validates the wallet name; `AddWallet(name, raw)`, used by `ImportWallet`, does not. The name goes straight into `Path.Combine(profile, name + ".uwa")` and the "already exists" test compares names in memory only.
**Abuse:** A name such as `x/../default` writes over the user's existing wallet file with attacker-supplied bytes; an absolute path writes a `.uwa` file anywhere the user can write. The original keys are gone unless backed up. Reachable without the password through A1.
**Direction:** Validate the name in one place for all paths, and refuse to overwrite an existing file.

#### A11. Wallet encryption has no integrity check — **Medium** · Verified
**Where:** [Wallet.cs:297](../Nexus/Wallet.cs#L297), [Wallet.cs:261](../Nexus/Wallet.cs#L261), [Wallet.cs:209](../Nexus/Wallet.cs#L209)
**What:** The wallet is encrypted with AES in its default CBC mode with no authentication tag. A wrong password is noticed only if the padding or the parsing happens to fail. If both pass (roughly one wrong attempt in tens of thousands), `Unlock` accepts it, keeps the wrong password as the new key, and the next save writes the wallet out with whatever was parsed. Saving is a direct overwrite, so a crash mid-write also loses the file.
**Direction:** Use authenticated encryption (AES-GCM), verify before accepting, write to a temporary file and rename.

#### A12. Password hashing can crash or stall the vault — **Medium** · Verified
**Where:** [Cryptography.cs:196](../Net/Cryptography.cs#L196), [VaultApi.cs:116](../Nexus/VaultApi.cs#L116)
**What:** `HashifyPassword` allocates `3 × password length` bytes on the stack, so a very long password overflows the stack and kills the process (this cannot be caught). Each attempt also runs Argon2id with 1 GiB of memory while holding the vault lock.
**Abuse:** One `UnlockWallet` request with a megabyte-long password (via A1) terminates the Nexus. Repeated normal attempts keep the vault busy; a member node that cannot get its votes signed misses rounds.
**Direction:** Cap password length before hashing, use a heap buffer, and rate-limit unlock attempts.

#### A13. Admin password handling — **Medium** · Verified
**Where:** [JsonServer.cs:17](../CS/JsonServer.cs#L17), [JsonServer.cs:199](../CS/JsonServer.cs#L199), [JsonServer.cs:291](../CS/JsonServer.cs#L291), [JsonClient.cs:82](../CS/JsonClient.cs#L82)
**What:** The password travels in the URL (`?credentials=`), unescaped; the full URL becomes the name of the request's log flow; the stored value is a single unsalted Blake3 hash; there is no attempt limit.
**Abuse:** The password ends up in logs and anything else that records URLs; a leaked hash (A7) is cheap to brute-force; guesses over the API are unlimited.
**Direction:** Send it in a header or body, store a salted slow hash, limit attempts.

#### A14. TLS certificates are not checked — **Medium** · Verified
**Where:** [Api.cs:27](../Net/Api.cs#L27)
**What:** The shared HTTP client used by all API clients accepts any server certificate.
**Abuse:** Whenever an API is used over HTTPS (the `Ssl` option for public addresses), anyone in the network path can read and alter the traffic, including the admin password in the URL.
**Direction:** Remove the override; pin or properly validate certificates.

#### A15. Secrets at rest and on the command line — **Medium** · Verified
**Where:** [McvPeering.cs:848](../Net/Mcv/McvPeering.cs#L848), [NexusSettings.cs:28](../Net/NexusSettings.cs#L28), [SecretKey.cs:52](../Net/User/SecretKey.cs#L52), [WalletCommand.cs:40](../Nexus/CLI/WalletCommand.cs#L40), [WalletCommand.cs:48](../Nexus/CLI/WalletCommand.cs#L48)
**What:** Vault session tokens are saved in plain text in the node and Nexus settings files. `SecretKey.ToString()` returns the private key, so any log line or string format that touches a key prints it — the `dev keypair` command already does: its line labelled "Public Address" prints the private key ([DevCommand.cs:74](../Nexus/Rdn/CLI/DevCommand.cs#L74)). The CLI takes wallet passwords and private keys as command-line arguments and prints newly created private keys to the console/log.
**Direction:** Protect or avoid persisting session tokens, make `ToString()` safe, read secrets from a prompt or a file, never print them.

#### A16. Error details, request size, test calls — **Low** · Verified
**Where:** [JsonServer.cs:240](../CS/JsonServer.cs#L240), [JsonServer.cs:285](../CS/JsonServer.cs#L285), [NodeApi.cs:58](../Net/Node/NodeApi.cs#L58)
**What:** Unhandled errors are returned to the caller as full exception text with stack traces. Request bodies have no size limit. A test call that only throws (`Exception`) is part of the production API.

#### A17. Secrets stay in memory — **Low** · Verified
**Where:** [Wallet.cs:241](../Nexus/Wallet.cs#L241), [ConsolePasswordAsker.cs:85](../CS/ConsolePasswordAsker.cs#L85)
**What:** Locking a wallet only drops references; key bytes and the derived AES key are not cleared. The console password helper keeps the last password in a public property and echoes the password in clear when a new one is created.

---

### B. Consensus engine (`Net/Mcv`)

#### B1. Energy can be created out of nothing, in two ways — **Critical** · Verified
**Where:** negative boost: [Round.cs:379](../Net/Mcv/Round.cs#L379), [Round.cs:389](../Net/Mcv/Round.cs#L389), validity check [Transaction.cs:44](../Net/Mcv/Transaction.cs#L44); genesis-key exemption: [UtilityTransfer.cs:97](../Net/Mcv/UtilityTransfer.cs#L97), [Mcv.cs:45](../Net/Mcv/Mcv.cs#L45), [UserCreation.cs:34](../Net/User/UserCreation.cs#L34)
**What:** (a) `Boost` is a signed number read from the transaction. Execution does `u.Energy -= t.Boost` and nothing checks its sign or the resulting balance (the signer is not put on the "spenders" list that is checked for negative balances). (b) `UtilityTransfer` does not deduct energy from the source when the signer's key equals the genesis ("God") key: `if(User.Key != Mcv.God.Puplic) { s.Energy -= Energy; … }` — but the receiver is still credited. That key's private half is a constant in the source (`[1, 0, 0, …]`), and nothing stops an account from being created with it.
**Abuse:** (a) A transaction with `Boost = -1,000,000,000,000` adds that much energy to the sender. (b) Anyone registers an account whose key is the genesis public key, signs with the published private key, and transfers any amount of energy to any account without owning it. Energy can be transferred on, pays for bandwidth and pays the candidacy fee, so both also buy consensus seats (B11). A large positive boost a user cannot afford leaves a negative balance and still wins pool priority.
**Direction:** Reject negative boost in `Valid` and check the signer's balance after charging it; remove the genesis-key exemption outside round 0 and forbid that key for accounts.

#### B2. A member can be accused of double-voting by anyone, without a signature — **Critical** · Verified (the expulsion step: Likely)
**Where:** [Mcv.cs:347](../Net/Mcv/Mcv.cs#L347) (fork test) comes before [Mcv.cs:360](../Net/Mcv/Mcv.cs#L360) (membership) and [Mcv.cs:368](../Net/Mcv/Mcv.cs#L368) (signature); duplicates are recognised only by signature bytes ([Mcv.cs:321](../Net/Mcv/Mcv.cs#L321))
**What:** When a second vote arrives for a generator that already voted in the round, the first vote is removed and the generator is added to `Forkers` — before the new vote's signature is looked at.
**Abuse:** After member M's real vote has spread, an attacker sends any vote carrying M's id and 64 arbitrary signature bytes. Every node that sees it drops M's vote and proposes M as a violator ([Round.cs:310](../Net/Mcv/Round.cs#L310), [Round.cs:283](../Net/Mcv/Round.cs#L283)); with enough proposals M is removed from the member list ([Round.cs:491](../Net/Mcv/Round.cs#L491)). Even short of that, nodes now disagree about which votes count, so rounds fail. An honest member that restarts and re-votes is treated the same way.
**Direction:** Verify membership and signature first; require two validly signed, different votes as proof of a fork.

#### B3. Transactions with an empty signature are treated as signed — **Critical** · Verified
**Where:** [Execution.cs:324](../Net/Mcv/Execution.cs#L324), [Transaction.cs:166](../Net/Mcv/Transaction.cs#L166), [McvPeering.cs:656](../Net/Mcv/McvPeering.cs#L656), [Extentions.cs:46](../Net/Extentions.cs#L46)
**What:** The signature check is skipped when `Signature == null` ("means synchronization"), and a zero-length signature on the wire is read as null. Such a transaction for any existing user passes `Examine` and enters a member's pool.
**Abuse:** (1) When the member next builds a vote, `NearestBy(null)` throws on the main thread and the node shuts itself down (B5) — one request per member stops the network. (2) A node synchronising from a hostile peer executes unsigned transactions for any account as genuine (C1).
**Direction:** Never accept a missing signature from the network; mark locally trusted replays some other way.

#### B4. A vote with an empty body stops member nodes — **Critical** · Verified
**Where:** [Vote.cs:166](../Net/Mcv/Vote.cs#L166), [Vote.cs:63](../Net/Mcv/Vote.cs#L63), [Mcv.cs:249](../Net/Mcv/Mcv.cs#L249), [Mcv.cs:368](../Net/Mcv/Mcv.cs#L368), [Round.cs:137](../Net/Mcv/Round.cs#L137), [McvPeering.cs:610](../Net/Mcv/McvPeering.cs#L610)
**What:** A vote is added to the round's list before it is checked. With a zero-length payload, computing the vote's hash throws. The first exception only drops the sender's connection, but the vote stays in the list, and every later re-check of the round throws again — on the main thread of a member node, which then aborts.
**Abuse:** One small packet naming any current member's id (public information) for the next voting round. Sent for a future round, it is also relayed by honest nodes (B6).
**Direction:** Validate and verify before storing; never keep a vote that failed a check.

#### B5. Any unexpected exception shuts the node down — **High** · Verified
**Where:** [Node.cs:61](../Net/Node/Node.cs#L61), [Node.cs:82](../Net/Node/Node.cs#L82)
**What:** Threads created by the node catch every exception and call `Abort`, which stops the node. Much of the code that handles data from peers can throw ordinary exceptions (null reference, missing element, bad cast, timeout). Examples that are reachable from the network: [McvPeering.cs:884](../Net/Mcv/McvPeering.cs#L884) (members answer of the wrong type), [McvPeering.cs:1075](../Net/Mcv/McvPeering.cs#L1075) (`First` on a tag the member invented), [Mcv.cs:417](../Net/Mcv/Mcv.cs#L417) (`MaxBy` on an empty set, see B14), the synchronisation thread (C5), and B3/B4/B8/B9.
**Abuse:** A fail-stop design means every such bug is a remote off-switch. Transactions that honest members pre-check with `Examine` are filtered, but anything a member puts into its own vote is executed by everyone without that filter (B7).
**Direction:** Contain exceptions per peer request and per transaction; fail the item, not the node.

#### B6. Unchecked votes are stored and relayed — **High** · Verified
**Where:** [Mcv.cs:306](../Net/Mcv/Mcv.cs#L306), [VotePpc.cs:37](../Net/Mcv/VotePpc.cs#L37), [Mcv.cs:513](../Net/Mcv/Mcv.cs#L513)
**What:** A vote for a round beyond the current voting round, or any vote received while the node is not synchronised, is stored without any check and reported as OK, which makes the node forward it to all its graph peers. Each new round number creates a round object; rounds are found by walking the list.
**Abuse:** Anyone can fill every node's memory with up-to-1 MB junk votes for arbitrary future rounds, and the network spreads them itself. Lookups slow down as the list grows.
**Direction:** Bound the round window and the number/size of pending votes, verify the signature against the current member set before storing or relaying.

#### B7. Transactions inside votes are never validated — **High** · Verified
**Where:** [Vote.cs:33](../Net/Mcv/Vote.cs#L33), [Mcv.cs:286](../Net/Mcv/Mcv.cs#L286), [Mcv.cs:374](../Net/Mcv/Mcv.cs#L374)
**What:** `Vote.Valid` loops over the vote's transactions, but it is called before the payload is parsed, when that list is still empty. After parsing, only the operation count and the "nearest member" rule are checked. `Transaction.Valid`, each operation's `IsValid` and the expiry rule are never applied to what members send each other.
**Abuse:** A single dishonest member can ship operations that break every size and format limit, or that make `Execute` throw on all nodes (B5).
**Direction:** Run full validation after `Restore`, and reject the vote if any transaction fails.

#### B8. Accounts can be created without a key, which later crashes every node — **High** · Verified
**Where:** [Execution.cs:312](../Net/Mcv/Execution.cs#L312), [UserCreation.cs:26](../Net/User/UserCreation.cs#L26), [User.cs:219](../Net/User/User.cs#L219), [Table.cs:199](../Net/Mcv/Table.cs#L199), [PublicKey.cs:112](../Net/User/PublicKey.cs#L112)
**What:** A transaction from an unknown user name creates the account with no signature check; the key is set only if the transaction contains `UserCreation`. A transaction with some other harmless operation (for example a zero self-transfer or a zero bandwidth allocation) leaves a permanent account whose key is null. Writing such an account to the table dereferences the null key.
**Abuse:** All nodes hit the same exception at the next table commit and again after every restart. Signature checks and key comparisons on that account throw as well. And because nothing in a first transaction is authenticated, the member that receives it can replace the key before including it, taking the name and anything else that transaction sets up.
**Direction:** Require the key in the creating transaction, and verify the creating transaction's signature against that key.

#### B9. Public keys are not validated — **High** · Verified
**Where:** [PublicKey.cs:73](../Net/User/PublicKey.cs#L73), [UserKeyChange.cs:28](../Net/User/UserKeyChange.cs#L28), [SecretKey.cs:44](../Net/User/SecretKey.cs#L44)
**What:** Any 32 bytes are accepted as a key. Verification later calls `ECXOnlyPubKey.Create`, which throws for values that are not valid curve points (about half of all random values).
**Abuse:** A member that changes its key to an invalid value makes every check of a vote carrying its id throw on every node (B5); anyone can send such votes.
**Direction:** Reject keys that do not parse, at the moment they are set.

#### B10. Operations are effectively free — **High** · Verified
**Where:** [Execution.cs:189](../Net/Mcv/Execution.cs#L189), [Operation.cs:59](../Net/Mcv/Operation.cs#L59), [McvPeering.cs:772](../Net/Mcv/McvPeering.cs#L772), [Transaction.cs:226](../Net/Mcv/Transaction.cs#L226), [Round.cs:425](../Net/Mcv/Round.cs#L425), [Rdn.cs:57](../Nexus/Rdn/Rdn.cs#L57), [Fair.cs:51](../Fair/cs/Fair.cs#L51)
**What:** `PayEnergy` only lowers a per-hour "rating"; nothing rejects a transaction when the rating goes below zero (the `NotEnoughBandwidth` message is never used). Proof of work is checked only if the sender chose to include one, so leaving it out passes; its difficulty is zero in the Simulation, Virtual and TA zones (172 in Test and Developer0); it is not tied to the transaction (one solution can be reused by anyone until the graph hash changes); and the client code that produces it never resets its hasher, so it would rarely produce a valid one. Failed transactions cost nothing.
**Abuse:** Unlimited free operations: spam, state growth, and cheap use of every other issue in this section.
**Direction:** Decide the fee model and enforce it in execution; bind proof of work to the transaction and require it when applicable.

#### B11. Consensus seats are cheap, and one bad declaration blocks all clients — **High** · Verified (takeover economics: To confirm)
**Where:** [CandidacyDeclaration.cs:30](../Net/Mcv/CandidacyDeclaration.cs#L30), [Round.cs:511](../Net/Mcv/Round.cs#L511), [Execution.cs:414](../Net/Mcv/Execution.cs#L414), [Net.cs:143](../Net/Mcv/Net.cs#L143), [McvPeering.cs:884](../Net/Mcv/McvPeering.cs#L884)
**What:** Any account that pays a one-off 1,000,000 energy becomes a candidate and is admitted as soon as there is room (up to 1,000 members). Newest candidates are admitted first; when the queue is full the oldest is dropped. There is no stake at risk. The declaration's endpoint list is not validated at all.
**Abuse:** (1) With B1 the fee is free; even without it, members earn the daily emission, so seats pay for themselves. Enough seats decide which 21 voters are drawn. (2) A member with an empty endpoint list makes every client's transaction loop skip forever (`cr.Members.Any(i => !i.GraphPpiEndpoints.Any())` → `continue`), so no non-member can submit transactions. (3) Endpoints can point at third parties.
**Direction:** Define real admission rules (stake, vetting or limits), admit oldest first, validate endpoints, and make clients skip unusable members instead of stopping.

#### B12. Off-chain check results are kept forever and resent in every vote — **High** · Verified
**Where:** [Mcv.cs:74](../Net/Mcv/Mcv.cs#L74), [McvPeering.cs:518](../Net/Mcv/McvPeering.cs#L518), [RdnNode.cs:44](../Nexus/Rdn/RdnNode.cs#L44), [FairNode.cs:52](../Fair/cs/FairNode.cs#L52), [McvNode.cs:71](../Net/Mcv/McvNode.cs#L71)
**What:** `Mcv.OutworldResults` is only ever added to, and the whole list is copied into each vote. `CurrentOutworlds`, meant to stop duplicate work, is never filled, so every confirmed round starts another DNS lookup for each pending request and may add duplicate results. Domain ownership is proven by a plain DNS TXT lookup through the system resolver.
**Abuse:** Each domain-migration or author-verification request permanently enlarges every future vote; because requests are free (B10) the votes can be pushed past the 1 MB packet limit, after which members cannot exchange votes until restart. Pending requests also make all members query the named domain every round. DNS answers that can be spoofed towards enough members approve a false ownership claim.
**Direction:** Remove results once confirmed or expired, track lookups in progress, cap pending requests per user, use validating resolvers.

#### B13. Quorum is `n × 2 / 3` rounded down — **Medium** · Verified
**Where:** [Round.cs:122](../Net/Mcv/Round.cs#L122), [Round.cs:91](../Net/Mcv/Round.cs#L91)
**What:** With 7 voters the quorum is 4, with 5 it is 3, with 10 it is 6. Two such quorums can overlap in a single voter, so one dishonest voter is enough to help confirm two different results. The usual rule is more than two thirds (`⌊2n/3⌋ + 1`).

#### B14. Votes claiming the genesis generator skip verification — **Medium** · Verified
**Where:** [Round.cs:141](../Net/Mcv/Round.cs#L141), [AutoId.cs:11](../Net/AutoId.cs#L11), [Mcv.cs:414](../Net/Mcv/Mcv.cs#L414)
**What:** When a round is re-evaluated, a vote whose generator id is −1 ("God") is marked OK without any check. Such votes do not count towards consensus, but they do count in `VotesOfTry`.
**Abuse:** A handful of forged votes pass the "enough votes" test while no real voter has voted; `MaxBy` then returns null and the next line throws (B5).
**Direction:** Accept the genesis generator only in genesis rounds created locally.

#### B15. Transaction pool eviction — **Medium** · Verified
**Where:** [McvPeering.cs:813](../Net/Mcv/McvPeering.cs#L813)
**What:** When the pool is full, the entry to evict is chosen with a lambda that reads the *new* transaction instead of the pool entry, so it is always the first one. The comparison adds the sender-chosen `Boost`, which is not charged against a real balance (B1).
**Abuse:** A sender with a large boost value can push other users' transactions out of a member's pool.

#### B16. Transaction status queries can be broken for everyone — **Medium** · Verified
**Where:** [TransactionStatusPpc.cs:18](../Net/Mcv/TransactionStatusPpc.cs#L18)
**What:** The lookup calls `i.Tag.SequenceEqual(...)` on every pooled transaction. A transaction submitted with an empty tag has a null `Tag`, and the call throws for every later query while it sits in the candidate or confirmed list. The number of tags per query is unbounded.

#### B17. Consensus time is not tied to local clocks — **Medium** · Likely
**Where:** [Round.cs:238](../Net/Mcv/Round.cs#L238)
**What:** Round time is the average of the largest group of voters' reported times and must only increase. Nodes never compare it with their own clock, so a quorum can move time far ahead (expiring rentals, bandwidth and domains) and it can never come back.

#### B18. Smaller observations — **Low / Info** · Verified
- Lock mix-ups: [PretransactingPpc.cs:11](../Net/Mcv/PretransactingPpc.cs#L11) locks the `Mcv` object instead of `Mcv.Lock`; `TimePpc`, `CostPpc`, `InfoPpc` read chain state with no lock.
- [Round.cs:139](../Net/Mcv/Round.cs#L139) compares a vote's `Try` before the vote is parsed, so stored-early votes always look like try 0.
- [McvPeering.cs:798](../Net/Mcv/McvPeering.cs#L798): the handler around `Examine` is `catch … when(!NodeGlobals.CatchCorrupted)`, which with the default setting catches nothing; a transaction that throws while being pre-checked drops the sender's connection instead of being answered with an error. The condition looks inverted. A few lines earlier, a transaction whose proof of work is wrong is skipped without an error, which the sender reads as "accepted".
- [PublicKey.cs:134](../Net/User/PublicKey.cs#L134) hash code uses `&` instead of `|` and is always 0; [Bytes.cs:54](../CS/Bytes.cs#L54) hashes byte arrays by their first byte only. Both make hash tables slow under chosen input.
- [Cryptography.cs:61](../Net/Cryptography.cs#L61) and [Cryptography.cs:81](../Net/Cryptography.cs#L81): `Hash(byte[])` is Blake2b while `Hash(ReadOnlySpan<byte>)` is Blake3. Which one a call uses depends on overload resolution; a compiler or language-version change could silently change consensus hashes.
- [Mcv.cs:45](../Net/Mcv/Mcv.cs#L45): the genesis ("God") private key is a fixed constant. Signing the locally built genesis rounds with it is harmless; the places where it keeps a special meaning afterwards are not (B1, B14).
- [Cryptography.cs:141](../Net/Cryptography.cs#L141): the Simulation zone uses a placeholder scheme whose signatures anyone can forge. It must never be reachable from outside a test machine.
- [NodeSettings.cs:39](../Net/Node/NodeSettings.cs#L39): global switches `ForceApproveOutworlds` and `ForceSynchronized` bypass verification and synchronisation. They are set only by the test runner today.

---

### C. Peer-to-peer networking and synchronisation

#### C1. A synchronising node believes whatever one peer tells it — **High** · Verified
**Where:** [McvPeering.cs:201](../Net/Mcv/McvPeering.cs#L201) (graph-state path: lines 288–305, chain path: lines 394–432), [Round.cs:732](../Net/Mcv/Round.cs#L732), peer choice [HomoPeering.cs:285](../Net/Node/HomoPeering.cs#L285)
**What:** In graph mode the tables and the state are downloaded from one peer and compared with hashes supplied by the same peer. In chain mode downloaded rounds are checked only for internal consistency; the stored round format contains no votes or member signatures, so history cannot be verified at all, and unsigned transactions are accepted (B3). The member set the node will later trust comes from that same data. There is no checkpoint or other fixed reference.
**Abuse:** A peer chosen for synchronisation can hand over an invented chain with its own member set and then supply "valid" votes for it. Peer choice is "first known peer with the role" and the peer list can be poisoned (C3).
**Direction:** Keep and verify quorum signatures for rounds, ship checkpoints, cross-check several peers.

#### C2. Lengths from the wire are used to allocate memory — **High** · Verified
**Where:** [Reader.cs:30](../CS/Reader.cs#L30), [Reader.cs:219](../CS/Reader.cs#L219), [Reader.cs:177](../CS/Reader.cs#L177), [Hello.cs:26](../Net/Node/Hello.cs#L26)
**What:** `ReadBytes`, `ReadUtf8`, `ReadArray`, `ReadList` and the rest read a 7-bit-encoded count and allocate that much before reading any element. The handshake `Hello` is read directly from the socket before anything is known about the peer.
**Abuse:** A few bytes make the node allocate up to 2 GB (or 8 GB for an int array) per connection or per packet; a slow sender keeps the buffer alive. Negative counts throw. Out-of-memory in one thread can take down others (B5).
**Direction:** Bound every count by the remaining input and by a per-field maximum.

#### C3. Anyone can rewrite the peer list — **High** · Verified
**Where:** [SharePeersPpc.cs:7](../Net/Mcv/SharePeersPpc.cs#L7), [HomoPeering.cs:218](../Net/Node/HomoPeering.cs#L218), [HomoPeering.cs:279](../Net/Node/HomoPeering.cs#L279), [TcpPeering.cs:333](../Net/Node/TcpPeering.cs#L333)
**What:** A peer may send up to 1,000 endpoints per message, as often as it likes. Unknown ones are added, saved to the database and forwarded to all other peers; for known ones the `Roles` value is overwritten with whatever was sent. Nothing is validated and the list has no size limit. A connecting peer also registers itself under a port of its own choosing.
**Abuse:** Fill every node's peer table with attacker addresses (which then win "first match" selection for calls and synchronisation), clear the roles of honest peers so they are never chosen, grow memory and disk without limit, and make the network open connections to third parties.
**Direction:** Limit and rate-limit gossip, never accept role claims about other peers, verify reachability before storing, cap the table, randomise selection.

#### C4. A half-closed connection spins a thread forever — **High** · Verified
**Where:** [Peer.cs:221](../Net/Node/Peer.cs#L221)
**What:** `do { s += Reader.Read(ReadBuffer, s, n - s); } while(s != n);` — when the remote side closes after announcing a length, `Read` returns 0 and the loop never ends.
**Abuse:** Each such connection burns one CPU core permanently; a dozen connections saturate a node.
**Direction:** Treat a zero-byte read as end of stream.

#### C5. Synchronisation dies on ordinary errors — **High** · Verified
**Where:** [McvPeering.cs:452](../Net/Mcv/McvPeering.cs#L452), [HomoPeer.cs:192](../Net/Node/HomoPeer.cs#L192), [HomoPeer.cs:208](../Net/Node/HomoPeer.cs#L208)
**What:** The synchronising thread handles five specific exception types. A peer that accepts a request and never answers causes a `TimeoutException` after 20 s, which is not among them, so the node aborts. A peer that disconnects mid-request causes an `OperationCanceledException`, which ends the thread quietly while the state stays "Downloading" — the node never tries again. Malformed answers throw other types with the same result.
**Direction:** Treat any failure as "try another peer".

#### C6. Connection limits — **Medium** · Verified
**Where:** [NodeSettings.cs:8](../Net/Node/NodeSettings.cs#L8), [Peer.cs:153](../Net/Node/Peer.cs#L153), [Peer.cs:171](../Net/Node/Peer.cs#L171), [TcpPeering.cs:178](../Net/Node/TcpPeering.cs#L178), [TcpPeering.cs:185](../Net/Node/TcpPeering.cs#L185)
**What:** Up to 16,384 inbound connections are accepted with no limit per address (a peer is identified by its IP plus a port it chooses itself). Each has its own thread and a 1 MB buffer. A peer that declares itself "permanent" gets no read timeout. A connection refused by `Consider` is not closed. Any error from `Accept` other than the two expected ones ends the listening thread and aborts the node.

#### C7. No encryption or peer authentication — **Medium** · Verified
**Where:** [TcpPeering.cs:195](../Net/Node/TcpPeering.cs#L195), [TcpPeering.cs:248](../Net/Node/TcpPeering.cs#L248)
**What:** Peer traffic is plain TCP. Votes and transactions are signed, but everything else (state download, member lists, nonces, peer lists) can be read and altered in transit. A node also learns its own public address from the first peer it talks to, unverified.

#### C8. Unsynchronised clients trust single answers — **Medium** · Verified
**Where:** [McvPeering.cs:1172](../Net/Mcv/McvPeering.cs#L1172), [McvPeering.cs:869](../Net/Mcv/McvPeering.cs#L869)
**What:** A node that is not synchronised asks one arbitrary graph peer for the member list, nonces, balances and costs and uses the answer as is. A false member list sends the user's transactions to the attacker; an answer of an unexpected type becomes null and crashes the client (B5).

#### C9. Request handling — **Low** · Verified
**Where:** [DownloadRoundsPpc.cs:27](../Net/Mcv/DownloadRoundsPpc.cs#L27), [Mcv.cs:535](../Net/Mcv/Mcv.cs#L535), [HomoPeer.cs:31](../Net/Node/HomoPeer.cs#L31)
**What:** Peer requests run on the connection thread, several of them while holding the chain lock, with no rate limit. `DownloadRounds` accepts any `From` and fills a cache limited by entry count (100,000) rather than bytes.

#### C10. State larger than one packet cannot be transferred — **Info** · Likely
**Where:** [Peer.cs:190](../Net/Node/Peer.cs#L190), [StampPpc.cs:11](../Net/Mcv/StampPpc.cs#L11), [DownloadClusterPpc.cs:26](../Net/Mcv/DownloadClusterPpc.cs#L26)
**What:** A graph state or table cluster above 1 MB makes the sender throw, after it has already written the packet header. New nodes would then be unable to synchronise.

---

### D. Inter-chain protocol (ICCP) and the local pipe

#### D1. The Nexus accepts ICCP from the whole network and relays it to local nodes — **High** · Verified
**Where:** [NexusSettings.cs:47](../Net/NexusSettings.cs#L47), [IccpPeering.cs:80](../Net/Iccp/IccpPeering.cs#L80), [IccpPeer.cs:86](../Net/Iccp/IccpPeer.cs#L86), [IccpLcpServer.cs:46](../Nexus/IccpLcpServer.cs#L46), [McvIccpLcpConnection.cs:186](../Net/Iccp/McvIccpLcpConnection.cs#L186), [McvIccpLcpConnection.cs:396](../Net/Iccp/McvIccpLcpConnection.cs#L396)
**What:** By default every Nexus listens on all interfaces. Inbound connections are unlimited, peers state themselves which nets they serve, and any request received is passed to the matching local node or, failing that, on to other peers. Local nodes answer `ShowGui` (opens the node window), `Do`, and `Request` (deserialise and run or forward an arbitrary peer request).
**Abuse:** End-user machines become open relays, can have windows popped up remotely, and inherit C2 and C4 on a port that is exposed by default. Relaying between peers has no loop protection.
**Direction:** Listen on loopback unless configured otherwise, limit inbound peers, restrict what remote callers may invoke.

#### D2. Inter-chain transfers are unfinished and unauthenticated — **Medium (must not ship as is)** · Verified
**Where:** [McvIccpLcpConnection.cs:110](../Net/Iccp/McvIccpLcpConnection.cs#L110), [IccpTransaction.cs:64](../Net/Iccp/IccpTransaction.cs#L64), [Transfer.cs:60](../Net/Iccp/Transfer.cs#L60)
**What:** A member that receives a transfer request fetches "the last outgoing transfer" from whichever peer claims to serve the source net and queues it for voting; the code says `TODO : Check signature`. Today the transfer result is not serialised and `AssetTransfer` does nothing on either side, so the path has no effect. Completed along the current lines, it would let anyone who reaches enough members credit assets that were never sent.

#### D3. Local pipe between Nexus and nodes — **Medium** · Verified
**Where:** [Lcp.cs:168](../Net/Lcp.cs#L168), [IccpLcpConnection.cs:22](../Net/Iccp/IccpLcpConnection.cs#L22), [Lcp.cs:57](../Net/Lcp.cs#L57), [IccpLcpServer.cs:29](../Nexus/IccpLcpServer.cs#L29), [NexusApi.cs:158](../Nexus/NexusApi.cs#L158)
**What:** The pipe has a predictable name (`0-iccp`), no access list, and does not insist on being the first instance. Clients are not authenticated: any local program can register as the node for any net with any API address, and `IccpNode` hands that address to the web UI. The server reads a new client's first fields inside the accept loop, so a client that connects and stays silent blocks all later connections, and one that disconnects early ends the accept thread.

#### D4. ICCP packet reading — **Low** · Verified
**Where:** [IccpPeer.cs:54](../Net/Iccp/IccpPeer.cs#L54)
**What:** The length is not checked for negative values and the body is read with a single `Read`, which may return fewer bytes than asked; the rest of the buffer then still holds data from earlier packets.

---

### E. Rdn chain logic (`Nexus/Rdn`)

Ownership checks on domains, names and resources are consistently tied to the signing account; apart from inbound links (E5) no way was found to change another owner's domain or resource directly. The problems are in value handling, storage and edge cases.

#### E1. Analysis rewards can create energy and spacetime — **Critical** · Verified
**Where:** [AnalysisResultUpdation.cs:44](../Nexus/Rdn/Analysis/AnalysisResultUpdation.cs#L44), [AnalysisResultUpdation.cs:71](../Nexus/Rdn/Analysis/AnalysisResultUpdation.cs#L71), [Analysis.cs:59](../Nexus/Rdn/Analysis/Analysis.cs#L59)
**What:** The reward amounts and the list of analyzers are read from resource data that any owner can write; nothing validates that data or even that the resource is an analysis. The reward is added to the analyzer and subtracted from the resource owner, but only the owner's balance is checked afterwards, and the amounts are signed.
**Abuse:** With a negative reward and two accounts of one's own (one listed as "analyzer", one owning the resource), the owner's energy and spacetime go up while the throw-away analyzer account goes negative unchecked. Unlike B1 this also creates spacetime, which pays for storage and names.
**Direction:** Validate analysis and council data when stored, reject negative rewards, check both parties.

#### E2. Reporting an analysis result changes shared data in place — **High** · Verified in code; effect Likely
**Where:** [AnalysisResultUpdation.cs:62](../Nexus/Rdn/Analysis/AnalysisResultUpdation.cs#L62), [ResourceData.cs:147](../Nexus/Rdn/Resource/ResourceData.cs#L147), [Resource.cs:94](../Nexus/Rdn/Resource/Resource.cs#L94)
**What:** `Resource.Clone()` copies the reference to `Data`, and `ResourceData.Get<T>()` caches the parsed object. `an.Results.Add(...)` therefore also changes the object held by the unmodified entity — including during `Examine`, whose result is supposed to be thrown away.
**Abuse:** The member that pre-checks such a transaction already has the result recorded when the round is executed, takes the "already reported" branch (no reward) and ends up with a different state from every other node; it then falls out of consensus and resynchronises. Sending such transactions to each member in turn disturbs all of them.
**Direction:** Never cache mutable parsed objects in shared entities; copy before changing.

#### E3. Domain names and domains are stored in the same database tables — **High** · Verified (naming); effect Likely
**Where:** [DomainNameTable.cs:11](../Nexus/Rdn/DomainName/DomainNameTable.cs#L11), [DomainTable.cs:11](../Nexus/Rdn/Domain/DomainTable.cs#L11), [Table.cs:17](../Net/Mcv/Table.cs#L17)
**What:** Both tables are constructed with the name `RdnTable.Domain`, and column-family names are derived from that name. In memory the two tables stay separate, but on disk they write cluster, bucket and entity records into the same column families, with keys of different types.
**Abuse:** Not an attack as such: once tables are committed to disk, a node that restarts reads mixed records — wrong entries, parse failures or an integrity error at start-up — while nodes that kept running do not. It has probably not shown up yet because commits happen only after 100,000 changes.
**Direction:** Give the name table its own name (`RdnTable.DomainName`), and add a migration.

#### E4. Reserved names are protected only by a local file check — **Medium** · Verified
**Where:** [DomainNameTable.cs:13](../Nexus/Rdn/DomainName/DomainNameTable.cs#L13), [DomainNameAcquisition.cs:14](../Nexus/Rdn/DomainName/DomainNameAcquisition.cs#L14)
**What:** The lists of existing web domains whose names are reserved are read from files in the node's data folder into a process-wide static, and consulted only in `IsValid`. `IsValid` is not applied to transactions carried in votes (B7), and each node uses whatever files it has.
**Abuse:** Any member, or any node whose files differ or are missing, lets acquisitions of reserved names through, and the rest of the network executes them.
**Direction:** Make the lists part of consensus data and enforce them in `Execute`.

#### E5. Resource links — **Medium** · Verified
**Where:** [ResourceLinkCreation.cs:37](../Nexus/Rdn/Resource/ResourceLinkCreation.cs#L37), [ResourceLinkCreation.cs:53](../Nexus/Rdn/Resource/ResourceLinkCreation.cs#L53), [ResourceUpdation.cs:127](../Nexus/Rdn/Resource/ResourceUpdation.cs#L127), [ResourceLinkDeletion.cs:38](../Nexus/Rdn/Resource/ResourceLinkDeletion.cs#L38)
**What:** (a) Only the source of a link must belong to the signer; anyone can add an inbound link to anyone's resource, and the list of inbound links has no limit, so a third party can make someone else's record grow without bound. (b) The link type is stored only inside the dependency branch, which is selected with `HasFlag` on an enum that is not a set of flags: `Organizational` and `Extra` links are stored with no type, and `Dns` (value 3) is treated as a dependency. (c) A "recursive" update follows organizational links to other resources without checking who owns them and with no depth limit. Because of (b) no such links can exist today, so (c) is dormant — fixing (b) alone would let one owner overwrite other owners' data and make deep chains overflow the stack. (d) Link deletion checks the index only against the upper bound; a negative index throws (B5).

#### E6. Renaming a domain leaves stale records — **Medium** · Verified
**Where:** [DomainRenaming.cs:28](../Nexus/Rdn/Domain/DomainRenaming.cs#L28)
**What:** The operation points the new name at the domain but does not clear the old name's pointer, does not check whether the new name is already attached to another domain, and does not re-index the domain's resources, which were registered under the old name. Afterwards the old name can never be attached to a new domain and the resources cannot be found by name.

#### E7. Smaller observations — **Low / Info** · Likely
- Storing resource data whose content is too short to hold a release address makes later operations throw while parsing it ([ResourceData.cs:93](../Nexus/Rdn/Resource/ResourceData.cs#L93)); transferring a sub-domain dereferences the parent name without a null check ([DomainTransfer.cs:54](../Nexus/Rdn/Domain/DomainTransfer.cs#L54)). Both are instances of B5.
- The domain-name pattern allows a leading dot and consecutive dots; resource names may contain `/`, control characters and up to 1,000 characters, so they do not survive a round trip through the address format ([DomainName.cs:11](../Nexus/Rdn/DomainName/DomainName.cs#L11), [Resource.cs:75](../Nexus/Rdn/Resource/Resource.cs#L75)).
- Owners' lists of domain names are not updated on transfer or re-acquisition (informational lists only).

---

### F. Release distribution and packages (`Nexus/Rdn/Release`, `Nexus/Package`)

Releases are sets of files identified by the hash of their index and downloaded from untrusted peers; packages are releases that get unpacked and started. The content of an index, a manifest or an archive is chosen entirely by whoever published it.

#### F1. Resource and package API calls read and write arbitrary local files — **Critical** · Verified
**Where:** [ResourceApi.cs:92](../Nexus/Rdn/Resource/ResourceApi.cs#L92), [ResourceApi.cs:198](../Nexus/Rdn/Resource/ResourceApi.cs#L198), [ResourceApi.cs:28](../Nexus/Rdn/Resource/ResourceApi.cs#L28), [PackageApi.cs:30](../Nexus/Package/PackageApi.cs#L30), [PackageApi.cs:143](../Nexus/Package/PackageApi.cs#L143), [ResourceHub.cs:229](../Nexus/Rdn/Resource/ResourceHub.cs#L229)
**What:** These calls need no password (A6). `LocalReleaseBuild` registers any local path as a release, keeping a pointer to the original file; `LocalReleaseRead` then returns its bytes. `ResourceDownload` and `PackageDeploy` write downloaded content to a path given in the `To` field. `PackageCreate` packs arbitrary local paths.
**Abuse:** A caller reads any file the user can read (settings with session tokens, wallet files, documents) — and because a built release is also announced to the network (F13), it can then be fetched by remote peers. A caller writes attacker-published content to any location, for example a start-up folder, without ever "running" a package.
**Direction:** Authenticate these calls, restrict paths to configured folders, and ask the user before exposing or writing files.

#### F2. Downloading a directory release can write files anywhere — **Critical** · Verified
**Where:** [Download.cs:326](../Nexus/Rdn/Release/Download.cs#L326), [Download.cs:354](../Nexus/Rdn/Release/Download.cs#L354), [LocalRelease.cs:142](../Nexus/Rdn/Release/LocalRelease.cs#L142)
**What:** File names come from the release index and are joined to the destination with `Path.Join(LocalPath, f.Name)`. Nothing rejects `..`, rooted paths or odd characters, and the write creates missing folders.
**Abuse:** A publisher puts names such as `../../…/Startup/x.bat` in the index. Anyone who downloads the release gets publisher-chosen content at that location; the hashes are in the same index, so verification passes.
**Direction:** Reject unsafe name segments and check that the full path stays under the release folder — both when downloading and when building an index.

#### F3. Package deployment extracts and deletes outside its folder — **High** · Verified
**Where:** [PackageHub.cs:444](../Nexus/Package/PackageHub.cs#L444), [PackageHub.cs:459](../Nexus/Package/PackageHub.cs#L459), [PackageHub.cs:474](../Nexus/Package/PackageHub.cs#L474)
**What:** Archive entry names are joined to the deployment folder and extracted with overwrite; lines of the `delete` entry are joined the same way and passed to `File.Delete`. No containment check, no size limit. For the complete archive every entry is extracted, not only those under `new/`.
**Abuse:** Installing a package — or receiving it as a dependency of another — lets its author overwrite or delete any file the user can, including other packages' files, before anything is run.
**Direction:** Canonicalise each target and require it to stay under the deployment root; cap sizes.

#### F4. Nothing decides whether a package may run — **High** · Verified
**Where:** [Nexus.cs:253](../Nexus/Nexus.cs#L253), [Nexus.cs:208](../Nexus/Nexus.cs#L208), [ResourceHub.cs:139](../Nexus/Rdn/Resource/ResourceHub.cs#L139), [RdnSettings.cs:75](../Nexus/Rdn/RdnSettings.cs#L75)
**What:** Between an address and `Process.Start` there is no signature, publisher trust or user confirmation. The start path and arguments come from the package's instruction file and go to the shell (`UseShellExecute = true`) without a containment check; the first start entry is used regardless of its platform condition; what runs is whatever sits in the folder at that moment. End-user nodes are configured by default without chain state, so the address is resolved by asking a single peer (C8), and the peer link is unprotected (C7).
**Abuse:** Besides A4: whoever answers the lookup, or sits in the network path, can substitute the release an address resolves to; the substituted content verifies against its own hashes and is then executed.
**Direction:** Trust and consent step before first run; resolve with proofs or several peers; contain the start path; no shell execution.

#### F5. Releases are marked complete before their files are — **High** · Verified
**Where:** [Download.cs:346](../Nexus/Rdn/Release/Download.cs#L346), [Download.cs:376](../Nexus/Rdn/Release/Download.cs#L376), [LocalRelease.cs:344](../Nexus/Rdn/Release/LocalRelease.cs#L344)
**What:** The directory download loop ends as soon as its queue is empty — while up to ten file downloads are still running, or after one has failed — and then marks the release `Full`. From then on `IsReady` answers "yes" for every file name, verification is skipped, and the node serves those files to others.
**Abuse:** A seeder that makes one download fail leaves the user with a release reported complete that is missing data or holds unverified data, which the user's node then passes on.
**Direction:** Complete a release only when every indexed file is verified; check per-file status when serving.

#### F6. Downloaded data is trusted until the very end — **High** · Verified
**Where:** [Download.cs:43](../Nexus/Rdn/Release/Download.cs#L43), [Download.cs:155](../Nexus/Rdn/Release/Download.cs#L155), [Download.cs:220](../Nexus/Rdn/Release/Download.cs#L220), [Download.cs:229](../Nexus/Rdn/Release/Download.cs#L229), [LocalRelease.cs:82](../Nexus/Rdn/Release/LocalRelease.cs#L82)
**What:** A piece is accepted on length alone and written straight into the final file. Only the whole file is hashed, and on mismatch the download starts over with no seeder penalised and no retry limit; the bad data stays on disk. The file length comes from the first seeder asked. For the index it is used to allocate a memory buffer.
**Abuse:** One hostile seeder can keep a download from ever finishing, fill the disk, leave its own bytes at the destination path, or make the downloader allocate gigabytes.
**Direction:** Put sizes and per-piece hashes in the index, verify before writing, write to a temporary file, score and drop bad seeders.

#### F7. Seed hubs accept any declaration and evict honest seeders — **Medium** · Verified
**Where:** [SeedHub.cs:53](../Nexus/Rdn/Release/SeedHub.cs#L53), [SeedHub.cs:95](../Nexus/Rdn/Release/SeedHub.cs#L95), [TcpPeering.cs:333](../Net/Node/TcpPeering.cs#L333)
**What:** Any connected peer may declare itself a seeder of any on-chain release without showing it has the data. A hub keeps 100 seeders per release and drops the oldest entries first. A peer's port is whatever it claimed, so one machine can register as many seeders as it likes. Entries never expire.
**Abuse:** Push every honest seeder of a chosen release out of its hubs so it cannot be found, or steer downloaders to the attacker (F6) or to third parties.
**Direction:** Proof of possession, per-address limits, expiry with re-declaration.

#### F8. File-serving requests are not bounded — **Medium** · Verified
**Where:** [DownloadReleasePpc.cs:12](../Nexus/Rdn/Release/DownloadReleasePpc.cs#L12), [LocalRelease.cs:161](../Nexus/Rdn/Release/LocalRelease.cs#L161)
**What:** Only the upper bound of `Length` is checked. `-1` means "whole file", so one request makes the seeder allocate and read an entire file while holding the hub lock. `Offset` is unchecked.

#### F9. Seed discovery can stop the node — **High** · Verified
**Where:** [SeedSeeker.cs:91](../Nexus/Rdn/Release/SeedSeeker.cs#L91), [SeedSeeker.cs:70](../Nexus/Rdn/Release/SeedSeeker.cs#L70), [RdnCandidacyDeclaration.cs:33](../Nexus/Rdn/RdnCandidacyDeclaration.cs#L33)
**What:** The discovery thread is a node thread (B5). It dereferences the member list returned by a peer and builds a hub object for each nearby member; a null or wrong-typed answer, or a member with no seed-hub endpoints (`IPs[0]`), throws. A candidacy declaration may carry an empty endpoint list — nothing validates it.
**Abuse:** A cheap consensus seat (B11) declared without seed-hub endpoints stops every node that starts a download of a release for which that member is one of the three nearest hubs.
**Direction:** Validate endpoints on chain, handle errors inside the loop.

#### F10. Dependency cycles crash the daemon — **Medium** · Verified
**Where:** [PackageHub.cs:86](../Nexus/Package/PackageHub.cs#L86), [PackageDownload.cs:51](../Nexus/Package/PackageDownload.cs#L51)
**What:** `IsReady` and the dependency counters recurse through a manifest's dependencies and parents with no visited set. A manifest that depends on itself, directly or in a loop, ends in a stack overflow, which terminates the process.

#### F11. Parsers trust depth and counts — **Medium** · Likely
**Where:** [Xon.cs:229](../CS/Xon/Xon.cs#L229), [XonBinaryReader.cs:127](../CS/Xon/XonBinaryReader.cs#L127), [Download.cs:324](../Nexus/Rdn/Release/Download.cs#L324)
**What:** Indexes and manifests fetched from peers are parsed recursively with no depth limit and with sizes taken from the data (see C2). Deep nesting overflows the stack; large counts cause large allocations.

#### F12. A dependency's pinned hash is not used — **Medium** · Likely
**Where:** [PackageDownload.cs:98](../Nexus/Package/PackageDownload.cs#L98)
**What:** Dependencies are downloaded by address, so they resolve to whatever the address points to at install time. The pinned release name stored in the manifest is never compared with what was resolved. If a dependency can be changed after publication, installers silently get different code.

#### F13. Privacy of local releases — **Low** · Likely
**Where:** [ResourceHub.cs:297](../Nexus/Rdn/Resource/ResourceHub.cs#L297), [LocalRelease.cs:237](../Nexus/Rdn/Release/LocalRelease.cs#L237)
**What:** Every local release, including ones built but never published, is declared to hubs and served to any peer that knows its address. Built releases serve the source files in place, so later changes to those files are served too. Everything downloaded is seeded automatically, and peers can ask which address holds which release.

#### F14. Smaller observations — **Low / Info** · Likely
- Stalls and busy loops: a piece is never reassigned while "in progress"; connector tasks end permanently on a timeout; several loops retry with no delay ([Download.cs:125](../Nexus/Rdn/Release/Download.cs#L125), [SeedSeeker.cs:138](../Nexus/Rdn/Release/SeedSeeker.cs#L138)).
- Deployment has no `try/finally`: a bad archive leaves the package "busy" forever, and a waiting loop then spins while holding the package lock ([PackageHub.cs:355](../Nexus/Package/PackageHub.cs#L355)).
- `HttpGet` serves published content from the API's own origin with a type guessed from the file extension ([RdnApi.cs:121](../Nexus/Rdn/RdnApi.cs#L121)).
- Piece offsets are multiplied in 32-bit arithmetic; files above about 2 GiB cannot complete ([Download.cs:40](../Nexus/Rdn/Release/Download.cs#L40)).
- The command line is re-joined and re-parsed, so quoting is lost and one argument containing spaces or `=` becomes several, including `profile=` and `zone=` ([Boot.cs:22](../CS/Boot.cs#L22)). Relevant to the `iccp:` link handler in A4.
- The release address namespace is called Blake3 but the hash is SHA-256 ([Cryptography.cs:124](../Net/Cryptography.cs#L124)).

---

### G. Fair chain logic (`Fair/cs`)

#### G1. Anyone can cast a moderator's vote — **Critical** · Verified
**Where:** [ProposalVoting.cs:67](../Fair/cs/Proposal/ProposalVoting.cs#L67), [FairOperation.cs:275](../Fair/cs/FairOperation.cs#L275); compare the publisher branch at [ProposalVoting.cs:59](../Fair/cs/Proposal/ProposalVoting.cs#L59)
**What:** For proposals decided by moderators, the vote names its voter in a `Voter` field, and the only check is that this id is one of the store's moderators. Nothing ties it to the account that signed the transaction. (The publisher branch does check ownership.)
**Abuse:** Any account can approve or reject any moderator-decided proposal in any store — its own reviews, registrations and publication requests included — and can vote "ban" against whoever raised a proposal, which blocks that moderator or publisher for 30 days. Under the default policies ([StoreCreation.cs:47](../Fair/cs/Store/StoreCreation.cs#L47)) almost every store action needs just one moderator vote. An approved publication request makes the requester a publisher; a publisher may propose deleting publications or removing other authors, again decided by one moderator vote; and publishers decide the surveys and referendums that remove moderators and change policies. The flaw is therefore a complete route to taking over any store.
**Direction:** Require the voter to be the signer (as `ProposalCreation` already does for moderators), and check the ban.

#### G2. Energy can be drained from a store, or created, through rewards and fees — **Critical** · Verified
**Where:** [AuthorUpdation.cs:63](../Fair/cs/Author/AuthorUpdation.cs#L63), [AuthorUpdation.cs:87](../Fair/cs/Author/AuthorUpdation.cs#L87), [FairExecution.cs:214](../Fair/cs/FairExecution.cs#L214), [ProposalCreation.cs:190](../Fair/cs/Proposal/ProposalCreation.cs#L190)
**What:** An author sets its own "moderation reward" to any signed number. Each moderated action on that author's publications then does `author.Energy -= reward; store.Energy += reward`, and only the author's balance is checked. With a negative reward the author gains and the store pays, with no check on the store. Separately, the candidate request fee is taken from the author and credited to the store without any balance check on the author.
**Abuse:** In a store the attacker moderates, every review or publication action mints energy for the attacker's author, which can be transferred out. In someone else's store it empties that store's energy. An author with no energy can still credit real energy to a store via the fee.
**Direction:** Reject negative rewards, check both sides of every transfer.

#### G3. Moving a category is not checked against loops or ownership — **High** · Verified
**Where:** [CategoryMovement.cs:28](../Fair/cs/Category/CategoryMovement.cs#L28), [CategoryMovement.cs:45](../Fair/cs/Category/CategoryMovement.cs#L45)
**What:** The operation checks that the category and the new parent exist and that both are in the same store as each other. It does not check that the category belongs to the store the proposal was made in, nor that the new parent is not the category itself or one of its descendants.
**Abuse:** A moderator of any store (anyone can create one) can detach or re-parent categories of another store, or create a loop. Loops break every piece of code that walks the tree (H1).
**Direction:** Verify the category's store, reject self and descendant parents.

#### G4. Stored proposals behave differently after a restart — **High** · Verified in code; how often it triggers: Likely
**Where:** [ReviewCreation.cs:28](../Fair/cs/Review/ReviewCreation.cs#L28), [ReviewEdit.cs:29](../Fair/cs/Review/ReviewEdit.cs#L29), [ProposalCreation.cs:130](../Fair/cs/Proposal/ProposalCreation.cs#L130), [ProposalVoting.cs:204](../Fair/cs/Proposal/ProposalVoting.cs#L204)
**What:** An operation's `User` is not part of what is stored. A node that created a proposal still holds the object with `User` set; a node that restarted or synchronised later loads it with `User` empty. The overlap test for reviews reads `User.Id` of the stored operation, and when a winning proposal is executed `User` is deliberately left empty for every role except "user".
**Abuse:** Two users proposing a review for the same publication make reloaded nodes throw where long-running nodes do not; with B5 the reloaded nodes stop. More generally, results that depend on what a node happens to have in memory are a route to nodes disagreeing.
**Direction:** Store what execution needs (the creator's id), and never rely on fields that are not serialised.

#### G5. A failed limit check is overwritten — **Medium** · Verified
**Where:** [ReviewCreation.cs:88](../Fair/cs/Review/ReviewCreation.cs#L88), [ReviewEdit.cs:64](../Fair/cs/Review/ReviewEdit.cs#L64), [FairExecution.cs:190](../Fair/cs/FairExecution.cs#L190)
**What:** `Allocate(..., out Error)` is followed directly by `RewardForModeration(..., out Error)`, which begins by clearing the error. If the publisher's space limit was exceeded, the allocation was skipped but the operation still counts as successful, so the text is stored without being paid for. Both helpers also index the publisher list with the result of `FindIndex` without checking for −1.

#### G6. "Majority" is half — **Medium** · Verified
**Where:** [ProposalVoting.cs:84](../Fair/cs/Proposal/ProposalVoting.cs#L84), [SurveyVoting.cs:75](../Fair/cs/Proposal/SurveyVoting.cs#L75)
**What:** The threshold is `n/2 + (n & 1)`: 1 of 2, 2 of 4, 3 of 6. With an even number of moderators or publishers, half is enough and whichever option gets there first wins.

#### G7. File type and size rules — **Medium** · Verified
**Where:** [FileCreation.cs:30](../Fair/cs/File/FileCreation.cs#L30)
**What:** `if(Data.Length > FileLengthMaximum && !Enum.IsDefined(Mime)) return false;` uses "and" where "or" was meant: the size limit applies only to undefined types, and undefined types are accepted when small. Files of the generic type can hold anything, which is what I1 builds on.

#### G8. Product fields can reference files that do not exist or belong to others — **Medium** · Verified
**Where:** [ProductUpdation.cs:74](../Fair/cs/Product/ProductUpdation.cs#L74), [TableState.cs:85](../Net/Mcv/TableState.cs#L85), [FileDeletion.cs:30](../Fair/cs/File/FileDeletion.cs#L30)
**What:** A file-id field is used without checking that the file exists or who owns it. A missing id makes `Affect` dereference null (the `if(x == null)` test after it can never be true). An existing id has its reference count increased whoever owns it.
**Abuse:** Any author can pin any other author's or store's file so that its owner can never delete it. The missing-file case is another instance of B5.

#### G9. Author rank is read from a local file while a round is being confirmed — **Medium** · Verified in code; deployment to confirm
**Where:** [AuthorVerification.cs:67](../Fair/cs/Author/AuthorVerification.cs#L67), [AuthorTable.cs:39](../Fair/cs/Author/AuthorTable.cs#L39)
**What:** When a web-domain verification succeeds, the author's rank is looked up in a file in the node's data folder and written into chain state (and copied to the author's publications). Nodes whose data files differ, or are missing, compute a different state.
**Direction:** Take the rank from consensus data, or pin the data set by hash.

#### G10. Review storage is booked to the author but released from the store — **Medium** · Verified
**Where:** [ReviewCreation.cs:88](../Fair/cs/Review/ReviewCreation.cs#L88), [ReviewTable.cs:46](../Fair/cs/Review/ReviewTable.cs#L46), [Execution.cs:276](../Net/Mcv/Execution.cs#L276)
**What:** Creating a review allocates space against the product's author; deleting a publication with its reviews frees that space from the store. The two counters drift apart, and `Free` throws an integrity exception when the store's counter would go below zero, which makes the deletion impossible through honest members and is another instance of B5.

#### G11. More operations that throw instead of failing — **Medium** · Verified
**Where:** [SurveyVoting.cs:58](../Fair/cs/Proposal/SurveyVoting.cs#L58), [StoreModeratorRemoval.cs:24](../Fair/cs/Store/StoreModeratorRemoval.cs#L24), [ProposalCreation.cs:29](../Fair/cs/Proposal/ProposalCreation.cs#L29), [ProposalCreation.cs:126](../Fair/cs/Proposal/ProposalCreation.cs#L126)
**What:** A survey vote with a negative choice passes validation and indexes the option list; removing a moderator uses `First` on a moderator who may already be gone; a proposal option whose operation is not a votable type is read as null and dereferenced; an operation type with no policy entry makes `First` throw. Each is harmless when honest members pre-check it and a network-wide stop when a member includes it directly (B5, B7).

#### G12. Smaller observations — **Low / Info** · Likely
- The overlap test for publication proposals compares store objects by reference, so it never detects a duplicate ([PublicationCreation.cs:29](../Fair/cs/Publication/PublicationCreation.cs#L29)).
- An author can change its own energy and spacetime limits on a store's publisher record ([AuthorUpdation.cs:93](../Fair/cs/Author/AuthorUpdation.cs#L93)); worth confirming that this is not meant to be the store's decision.
- The vector-search (HNSW) index is not reachable: its indexing method throws "not implemented" and its only user is commented out.

---

### H. Fair web API (`Fair/cs/WebServer`, `Web/`)

The API is read-only, has no accounts and serves files only as JPEG, PNG or a generic download, which removes many common web problems. What remains is mostly about a public endpoint running inside the same process as the chain node.

#### H1. A category that is its own parent takes the node down with one request — **High** · Verified
**Where:** [CategoryMovement.cs:45](../Fair/cs/Category/CategoryMovement.cs#L45), [SearchService.cs:36](../Fair/cs/WebServer/Services/SearchService.cs#L36), [SearchService.cs:107](../Fair/cs/WebServer/Services/SearchService.cs#L107), [CategoriesService.cs:64](../Fair/cs/WebServer/Services/CategoriesService.cs#L64)
**What:** Moving a category does not check that the new parent is not the category itself or one of its descendants. The web services walk category trees by recursion or by following parents in a loop, with no visited set and no depth limit, and the `categoriesIds` query parameter is not checked against the store in the path.
**Abuse:** Anyone can create a store, create a category and make it its own parent. A search request naming that category then recurses until the stack overflows, which terminates the whole process — web server and chain node together; the category-details request loops until memory runs out. If the node is also a consensus member, the member goes down with it.
**Direction:** Reject cycles on chain; walk trees iteratively with a visited set and hard limits; require requested categories to belong to the store.

#### H2. The web API reads chain state without the chain lock — **High** · Verified (how often it bites: To confirm)
**Where:** no `lock` anywhere under `Fair/cs/WebServer`, e.g. [FilesService.cs:114](../Fair/cs/WebServer/Services/FilesService.cs#L114); shared cache in [Table.cs:153](../Net/Mcv/Table.cs#L153); [NGram.cs:20](../Fair/cs/Search/NGram.cs#L20)
**What:** Everywhere else, chain state is read under `Mcv.Lock`. The web services read it from request threads with no lock, while the consensus thread changes it. Table lookups fill a shared cache lazily and publish an entity before it has been fully read. The n-gram entity has a finalizer that dereferences a field which stays null if the object was never loaded.
**Abuse:** Under concurrent requests the consensus thread can pick up a half-read entity and compute a different state from the rest of the network (the node then drops out and resynchronises), a request can fail on a collection modified underneath it, and an orphaned n-gram object can throw in its finalizer, which ends the process.
**Direction:** Serve from an immutable snapshot or under a reader lock with bounded work; publish cache entries only when complete; remove the finalizer.

#### H3. Search does unbounded work per request — **Medium** · Verified (validation); Likely (cost)
**Where:** [SearchQueryValidator.cs:8](../Fair/cs/WebServer/Validators/SearchQueryValidator.cs#L8), [StoreSearchQueryValidator.cs:8](../Fair/cs/WebServer/Validators/StoreSearchQueryValidator.cs#L8), [SearchService.cs:27](../Fair/cs/WebServer/Services/SearchService.cs#L27), [NGram.cs:124](../Fair/cs/Search/NGram.cs#L124)
**What:** A search query is only checked for being non-empty (the store-search validator is an empty TODO). The index does one lookup per query character, repeated for every nested category or product type, then loads publication, product and author for every candidate, sorts them all and only then applies paging. The cancellation token does not reach these loops.
**Direction:** Minimum and maximum query length, stop collecting after the requested window, cap page depth, pass the token through.

#### H4. No request limits on a server that shares a process with consensus — **Medium** · Verified
**Where:** [WebServer.cs:28](../Fair/cs/WebServer/WebServer.cs#L28), [FairSettings.cs:23](../Fair/cs/FairSettings.cs#L23)
**What:** The host is built with framework defaults: no rate limiter, no cap on connections or concurrent requests, no request timeout, 30 MB request bodies accepted on an API that only serves GET and HEAD, no response caching. Expensive requests (H3, H5) compete directly with the chain threads.
**Direction:** Rate and concurrency limits, request timeouts and Kestrel limits, configurable in the web settings; or state clearly that a reverse proxy must provide them.

#### H5. Whole collections are loaded to return one page or a count — **Medium** · Likely
**Where:** [FilesService.cs:66](../Fair/cs/WebServer/Services/FilesService.cs#L66), [ModeratorProposalsService.cs:191](../Fair/cs/WebServer/Services/ModeratorProposalsService.cs#L191), [PublicationsService.cs:123](../Fair/cs/WebServer/Services/PublicationsService.cs#L123), [PublishersService.cs:45](../Fair/cs/WebServer/Services/PublishersService.cs#L45)
**What:** Listing a store's or author's files loads every file including its data just to count them. Proposal lists load every proposal of the store and search title and text with a `search` value of unlimited length. Filtering publications by author walks every category and loads every publication and product in the store. Several list endpoints load all of a store's proposals once per item on the page.
**Direction:** Page over the id lists and load only the requested window; build per-request lookups once; cap `search` length.

#### H6. One stale or unusual record turns a whole page into an error — **Medium** · Verified for the cases checked
**Where:** [McvUtils.cs:47](../Fair/cs/WebServer/Utils/McvUtils.cs#L47), [ProposalUtils.cs:154](../Fair/cs/WebServer/Utils/ProposalUtils.cs#L154), [UserRegistrationModel.cs:5](../Fair/cs/WebServer/Models/VotableOperations/UserRegistrationModel.cs#L5), [PublicationsService.cs:357](../Fair/cs/WebServer/Services/PublicationsService.cs#L357), [SearchService.cs:228](../Fair/cs/WebServer/Services/SearchService.cs#L228)
**What:** Lookups that return nothing for deleted entities are used without a check, and lists keep ids of deleted items (a store's changed-publications list, open proposals that outlive what they refer to). The creator lookup throws for the `Candidate` role, which the default policy allows for publication requests. Proposal details read `operation.Store` and `operation.User`, which are not stored and are empty after a restart (G4). A numeric user search dereferences a user that may not exist.
**Abuse:** A single such record — which any user can create, for example a publication request made as a candidate — makes the corresponding moderation list fail for every moderator.
**Direction:** Check every lookup, skip or flag stale rows, derive values from stored data, clean the lists when entities are deleted.

#### H7. Errors raised while the response is written bypass the error handling — **Medium** · Verified (wiring); Likely (effect)
**Where:** [HttpResponseExceptionFilter.cs:12](../Web/Filters/HttpResponseExceptionFilter.cs#L12), [ExceptionTransformMiddleware.cs:13](../Web/Middlewares/ExceptionTransformMiddleware.cs#L13), [WebServer.cs:54](../Fair/cs/WebServer/WebServer.cs#L54), [ProductsService.cs:183](../Fair/cs/WebServer/Services/ProductsService.cs#L183)
**What:** Errors are translated by an action filter only; the exception middleware in `Web/` is never registered and there is no outermost handler. Many services return lazy sequences, so lookups and model building run during JSON serialisation, after the filter has finished. A failure there produces a bare 500 or a cut-off response instead of the API's error format.
**Direction:** Materialise results inside the services and add one outermost exception handler.

#### H8. Input validation gaps — **Low** · Verified for the cases checked
**Where:** [AutoIdValidator.cs:10](../Fair/cs/WebServer/Validators/AutoIdValidator.cs#L10), [PaginationValidator.cs:10](../Fair/cs/WebServer/Validators/PaginationValidator.cs#L10), [Regexes.cs:7](../Fair/cs/WebServer/Constants/Regexes.cs#L7), [StoreUsersService.cs:56](../Fair/cs/WebServer/Services/StoreUsersService.cs#L56)
**What:** Ids are accepted by a plain number parse, so negative values (including the reserved ones), signs and surrounding whitespace pass. `page` has no upper bound and `page × pageSize` is computed in 32-bit arithmetic, so large values wrap around. The user-name pattern omits `_`, which the chain allows, so such users cannot be looked up by name. An empty store-user search reaches an argument check and returns 500 instead of 400. The regular expressions themselves are linear and safe.

#### H9. Response and host hardening — **Low** · Likely
**Where:** [WebServer.cs:25](../Fair/cs/WebServer/WebServer.cs#L25), [HttpResponseExceptionFilter.cs:83](../Web/Filters/HttpResponseExceptionFilter.cs#L83), [FilesService.cs:120](../Fair/cs/WebServer/Services/FilesService.cs#L120), [appsettings.json:8](../Fair.CLI/appsettings.json#L8), [InvalidAutoIdException.cs:15](../Fair/cs/WebServer/Exceptions/InvalidAutoIdException.cs#L15)
**What:** No security headers are sent for the UI or the API (content-security policy, framing, `nosniff`, referrer policy) and the server banner is on. Files and avatars have no cache validators, so the full content is resent every time; generic-type files are served inline rather than as downloads (see I1); avatars are always labelled PNG. Any `Host` header is accepted. In the Simulation, Virtual and TA zones the host runs as "Development": error bodies include stack traces and the framework's developer error page answers anything the filter misses. Error messages repeat raw input without a length limit. Every client error is logged as a warning with the full exception, an aborted request is logged as a server error, and with web logging switched off real errors leave no trace.

#### H10. Moderation and user endpoints are public — **Info** · Verified
**Where:** all controllers under [Controllers/Moderation](../Fair/cs/WebServer/Controllers/Moderation), [WebServer.cs:71](../Fair/cs/WebServer/WebServer.cs#L71), [ProposalModel.cs:11](../Fair/cs/WebServer/Models/Proposals/ProposalModel.cs#L11), [UserModel.cs:24](../Fair/cs/WebServer/Models/UserModel.cs#L24)
**What:** All 57 routes are anonymous. The moderation routes return review text still awaiting moderation, unpublished publications, each voter's choice including ban votes, and publishers' limits and ban dates. User routes return the public key, favourite stores and store memberships. All of this is on chain anyway, so it is a decision to confirm rather than a leak.

#### H11. Smaller observations — **Info** · Likely
- Moderator surveys compute a page but return everything ([ModeratorSurveysService.cs:32](../Fair/cs/WebServer/Services/ModeratorSurveysService.cs#L32)); several totals do not match the filtered data ([SearchService.cs:182](../Fair/cs/WebServer/Services/SearchService.cs#L182)); list items carry full voter lists and texts ([ProposalModel.cs:11](../Fair/cs/WebServer/Models/Proposals/ProposalModel.cs#L11)).
- The configurable CORS set-up in `Web/` is never called, so Release builds have no way to allow an origin ([ConfigureCorsPolicy.cs:11](../Web/Configurations/ConfigureCorsPolicy.cs#L11)).
- Title search collects hits in a sorted set whose comparer looks only at rank and distance, so two different results that tie are treated as one ([PublicationTitleNgramIndex.cs:118](../Fair/cs/Publication/PublicationTitleNgramIndex.cs#L118)).
- Deleting a publication leaves its id in the store's changed-publications list and leaves open proposals pointing at it ([PublicationTable.cs:52](../Fair/cs/Publication/PublicationTable.cs#L52)); the product type is not validated when a product is created ([ProductCreation.cs:15](../Fair/cs/Product/ProductCreation.cs#L15)).
- Lower-casing and number parsing on request threads use the machine's culture ([SearchService.cs:234](../Fair/cs/WebServer/Services/SearchService.cs#L234)); the sample `.http` files and launch profiles refer to routes and formats that no longer exist.

---

### I. Fair web UI (`Fair/WebUI`)

No place was found where on-chain text is inserted as HTML or script: there is no `dangerouslySetInnerHTML`, `innerHTML`, `eval` or similar, markdown is rendered with HTML disabled, and files are served only as JPEG, PNG or a generic download type. The issues are about where requests go and what gets signed.

#### I1. Ids from the address bar go straight into API paths, and the answer decides what is signed — **Medium** · Verified for the `productId` path; Likely for the others
**Where:** [CreatePublicationPage.tsx:37](../Fair/WebUI/src/ui/pages/moderation/publications/CreatePublicationPage.tsx#L37), [getFairApi.ts:338](../Fair/WebUI/src/api/fair/getFairApi.ts#L338), [utils.ts:66](../Fair/WebUI/src/api/utils.ts#L66), [regexes.ts:1](../Fair/WebUI/src/utils/regexes.ts#L1), [FileCreation.cs:30](../Fair/cs/File/FileCreation.cs#L30)
**What:** Route and query ids are interpolated into API URLs without validation or encoding (an id pattern exists but is never used). The response is parsed as JSON whatever its type, and the operation the user then signs is built from the `id` inside that response. Separately, the chain accepts files of any type, and they are served from the same API.
**Abuse:** An attacker uploads a file containing JSON, then sends a moderator a link such as `…/publications/new?productId=../../../../../files/<id>`. The browser resolves the path to the attacker's file; the page shows a made-up product card, and "Create" signs a publication for whatever product id the file names. The same pattern applies to the changed-publication and proposal pages (the vote goes to the proposal named in the file while the text shown is the attacker's), and without any signing a link can show a fake publication inside a real store's branding.
**Direction:** Validate ids against the pattern, encode every path segment, require a JSON content type, and build operations from the validated route id.

#### I2. The VK video embed frames any site — **Medium** · Verified
**Where:** [VideoViewVk.tsx:13](../Fair/WebUI/src/ui/components/VideoView/VideoViewVk.tsx#L13), [videoUtils.ts:4](../Fair/WebUI/src/utils/videoUtils.ts#L4), [VideoFrame.tsx:9](../Fair/WebUI/src/ui/components/VideoFrame.tsx#L9)
**What:** A product's video link is recognised as "VK" by a pattern that is not anchored to the start of the URL, and when the path contains `video_ext.php` the embed address is rebuilt from the link's own host. The frame has no `sandbox` attribute and is allowed to write the clipboard.
**Abuse:** An author sets the video link to `https://evil.example/vk.com/video_ext.php?oid=1`. Everyone who opens the publication, and every moderator reviewing it, loads the attacker's page inside the store page, where it can show a fake sign-in or download prompt, redirect the tab after a click, and talk to the local daemons (A5).
**Direction:** Compare the parsed host name exactly, always rebuild the embed on a fixed host, sandbox the frame.

#### I3. The addresses of the local vault and node come from the remote API — **Medium** · Verified in the UI; impact depends on deployment
**Where:** [getLocalFairApi.ts:5](../Fair/WebUI/src/api/localFair/getLocalFairApi.ts#L5), [AuthenticationProvider.tsx:64](../Fair/WebUI/src/app/AuthenticationProvider.tsx#L64), [getIccpNodeApi.ts:34](../Fair/WebUI/src/api/iccpNode/getIccpNodeApi.ts#L34), [NodeController.cs:13](../Fair/cs/WebServer/Controllers/NodeController.cs#L13)
**What:** The UI asks the Fair API where the user's vault and Nexus are and uses the answers without checking that they are local addresses. On page load it sends the stored session to the "vault" address, and it sends every transaction with the session to the "node" address.
**Abuse:** A hostile or compromised API host, or anyone on the network path when the API is reached over plain HTTP, receives users' sessions and transactions and can answer "confirmed" for transactions it never forwarded.
**Direction:** Use fixed loopback addresses in the UI, or at least insist on a loopback host.

#### I4. Links stored on chain are loaded as images and media — **Low** · Verified
**Where:** [buildMediaItems.ts:19](../Fair/WebUI/src/ui/views/PublicationContentView/content/utils/buildMediaItems.ts#L19), [SlideContent.tsx:25](../Fair/WebUI/src/ui/components/Slider/SlideContent.tsx#L25), [AboutInfo.tsx:22](../Fair/WebUI/src/ui/components/specific/AboutInfo.tsx#L22)
**What:** Any link in a product's media fields that is not recognised as a video is rendered as an image, and store descriptions may contain markdown images. Every viewer's browser requests those addresses.
**Abuse:** Tracking of viewers, and — pointed at a loopback address — the body-less requests described in A5.

#### I5. Sessions in the browser — **Low** · Verified
**Where:** [AuthenticationProvider.tsx:51](../Fair/WebUI/src/app/AuthenticationProvider.tsx#L51), [VaultApi.ts:3](../Fair/WebUI/src/api/vault/VaultApi.ts#L3)
**What:** Vault sessions for all remembered accounts are kept in `localStorage` with no expiry. Removing an account only deletes the local copy; the vault is never told (and has no call for it, see J3).

#### I6. Smaller observations — **Low / Info** · Likely
- Translated proposal texts insert on-chain titles with escaping switched off, so a title can add formatting or an extra link to what voters read ([utils.tsx:44](../Fair/WebUI/src/ui/views/Proposal/content/utils.tsx#L44), [i18n/index.ts:9](../Fair/WebUI/src/i18n/index.ts#L9)).
- The search highlighter builds a regular expression from the query text; a crafted link, or simply typing `c++`, breaks the page or hangs the tab ([HighlightText.tsx:16](../Fair/WebUI/src/ui/components/HighlightText.tsx#L16)).
- Author profile links accept any scheme and pick the brand icon by substring, so `https://evil.example/github.com/…` is shown as a GitHub button ([ProfileLink.tsx:15](../Fair/WebUI/src/ui/components/author/ProfileLinks/ProfileLink.tsx#L15)).
- No content-security policy, framing protection or referrer policy; fonts are loaded from Google ([index.html](../Fair/WebUI/index.html)).
- Votes, joins and favourites are signed on a single click with no confirmation in the UI ([renderActionButtons.tsx:13](../Fair/WebUI/src/ui/renderers/renderActionButtons.tsx#L13)).
- At sign-in the key to authorise is the one the remote node reports for the typed name; a mismatch is detected only after the vault has already issued a session, which is not revoked ([getVaultApi.ts:21](../Fair/WebUI/src/api/vault/getVaultApi.ts#L21)).
- Download buttons hide their destination and pass `iccp:` and `magnet:` links to the operating system's handlers ([components.tsx:36](../Fair/WebUI/src/ui/components/publication/SoftwareInfo/components.tsx#L36)); see A4 for what an `iccp:` link can do.
- A debug panel is rendered unconditionally on the create-proposal page ([CreateProposalView.tsx:165](../Fair/WebUI/src/ui/views/CreateProposal/CreateProposalView.tsx#L165)).
- Dependencies of `Fair/WebUI` showed nothing known-bad in the lock file, but no online audit could be run.

---

### J. Desktop hosts, tooling, repository and dependencies

#### J1. Android signing keystore and its passwords are in the repository — **High** · Verified
**Where:** [Umc/myapp.keystore](../Umc/myapp.keystore), [Uccs.Umc.csproj:326](../Umc/Uccs.Umc.csproj#L326)
**What:** The release-signing keystore for the mobile app (`uo.umc`) is committed, and the project file next to it contains the store and key passwords in clear. Both have been in the history since 2023.
**Abuse:** Anyone with access to the repository or its history can sign an APK that Android accepts as an update of any build distributed with this key.
**Direction:** Treat the key as compromised, rotate it, keep signing material in CI secrets, remove it from history before the repository is shared.

#### J2. `iccp:` links start a second Nexus with a chosen profile and zone — **Medium** · Likely (delivery through browsers to confirm)
**Where:** [NexusWindows.cs:247](../Nexus.Windows/NexusWindows.cs#L247), [Boot.cs:22](../CS/Boot.cs#L22), [NetBoot.cs:18](../Net/NetBoot.cs#L18)
**What:** The link handler is registered as `"<nexuswin.exe>" open "%1"`. The Windows host does not implement `open`; it just starts, joins its arguments back into one string and parses them as Xon. In that syntax `{}` separates nodes, so a link such as `iccp:x{}profile=<path>{}zone=Simulation` yields `profile` and `zone` settings without needing a space.
**Abuse:** A link starts a full second Nexus on an attacker-chosen profile folder (possibly a network share) and zone, creating files there and loading whatever settings and wallets it finds.
**Direction:** Pass `%1` as one opaque value to a small forwarder that validates it and hands it to the running instance.

#### J3. Sessions cannot be revoked and never expire — **Medium** · Verified
**Where:** [SessionsPage.cs:60](../Nexus.Windows/SessionsPage.cs#L60)
**What:** The Revoke button looks the wallet up with `Wallets.SelectedItem as string`, but the list holds `Wallet` objects, so the lookup returns null and the handler throws. No granted session — including "always allow" ones — can be withdrawn from the UI, and sessions have no lifetime.

#### J4. Approval dialogs — **Medium** · Verified
**Where:** [AuthenticattionForm.cs:13](../Nexus.Windows/AuthenticattionForm.cs#L13), [AuthorizationForm.cs:13](../Nexus.Windows/AuthorizationForm.cs#L13), [VaultApi.cs:209](../Nexus/VaultApi.cs#L209), [JsonServer.cs:285](../CS/JsonServer.cs#L285)
**What:** In addition to A8 and A9: the vault lock is held while a dialog is open, so one unanswered prompt blocks every other vault request, including a member node's vote signing. A request with no body is enough to open the authentication dialog (the server builds a default call), so even an image tag on a web page can do it; there is no limit on how many queue up. Prompts raised by the node for transfers and key changes show an empty description. The description box is small with no scroll bar, so long text is cut. The requester's logo is decoded with GDI+ at any size before consent. "Allow always" is the first button and there is no delay before it becomes active. Dialogs have no owner window and can open behind others.
**Direction:** Do not hold the lock across UI, queue prompts with limits per requester, require a proper POST, render caller text as secondary and bounded.

#### J5. One genesis ("Father") key for every zone — **Medium** · Likely
**Where:** [Net.cs:149](../Net/Mcv/Net.cs#L149), [OsGroup.cs:107](../TestAutomation/OsGroup.cs#L107)
**What:** The public key of the first account and first consensus member is a single constant shared by Simulation, Virtual, TA, Test and Developer0, and would be inherited by a main zone. The automated tests sign as that account, so its private key lives on developer machines (in a project outside this repository). Whoever can read that project controls the genesis account of the public Test zone.
**Direction:** One key per zone; generate non-test keys offline and never reuse test keys.

#### J6. Wallet screens — **Low** · Verified
**Where:** [WalletsPage.cs:212](../Nexus.Windows/WalletsPage.cs#L212), [WalletsPage.cs:258](../Nexus.Windows/WalletsPage.cs#L258), [Wallet.cs:277](../Nexus/Wallet.cs#L277), [SessionsPage.cs:39](../Nexus.Windows/SessionsPage.cs#L39)
**What:** Deleting a wallet is one click with no confirmation, password or backup, and works on locked wallets. Rename and import do not validate the name (a label with `..\` moves the wallet out of the profile). Any non-blank password is accepted. "Show secret" compares two byte arrays by reference, so it always refuses — safe today, but a careless fix could remove the check. Session tokens, which are bearer credentials, are shown in full. "Import key" collects a private key and discards it.

#### J7. Unlock prompt — **Low** · Verified
**Where:** [NexusWindows.cs:165](../Nexus.Windows/NexusWindows.cs#L165), [Vault.cs:222](../Nexus/Vault.cs#L222)
**What:** Any session holder can make a password prompt appear by asking a locked wallet to sign; the prompt does not say who asked. Retries are unlimited, only one exception type is handled, and an unlocked wallet stays unlocked until the user locks it or exits.

#### J8. Runtime and dependencies — **Medium** · To confirm
**Where:** [Uccs.Net.csproj:31](../Net/Uccs.Net.csproj#L31), all project files
**What:** All projects target .NET 9, a short-term-support release whose support window ends around the date of this review — worth checking against Microsoft's lifecycle page and planning the move to .NET 10. The core assembly links a beta build of BouncyCastle, used only for random bytes and hex encoding that the framework already provides. The mobile app targets .NET 6/7, both out of support, with several preview packages. There are no NuGet lock files. `Web.UI` pins tooling versions (vite 6.0.7, esbuild 0.24.2, lodash 4.17.21) that have published advisories, mostly affecting development servers. I could not check advisories online; `dotnet list package --vulnerable` and `yarn audit` will give the authoritative list.

#### J9. `Nas.sol` — **Medium / Info (currently unused)** · Verified
**Where:** [Nas.sol:22](../Nas.sol#L22), [Nas.sol:48](../Nas.sol#L48)
**What:** No C# code refers to the contract any more. If it is still meant to be used: the zone/seed registry is controlled by one fixed address with no transfer of ownership, multisig, delay or events, so a single key can silently replace the bootstrap list and its loss freezes the registry. `Emit` records the first payment made under a caller-chosen "secret" that was derived from predictable data, so anyone can register it first with a tiny amount and block or distort the real one. The compiler version is unbounded (`>=0.7.4`).

#### J10. Debug and Release builds speak different formats — **Low** · Verified
**Where:** [Transaction.cs:106](../Net/Mcv/Transaction.cs#L106), [Peer.cs:54](../Net/Node/Peer.cs#L54), [WebServer.cs:38](../Fair/cs/WebServer/WebServer.cs#L38)
**What:** Debug builds append an 8-byte marker to every packet and stored transaction, so they cannot interoperate with Release peers or databases, and the Fair web API allows every origin in Debug.

#### J11. Test tooling — **Low** · Likely
**Where:** [Program.cs:77](../TestAutomation/Program.cs#L77), [run.sh](../TestAutomation/run.sh), [Program.cs:62](../TestAutomation/Program.cs#L62)
**What:** Every test run begins by emptying a folder whose path comes from outside the repository, and the launch script runs the whole thing with `sudo -E`; a wrong or empty variable deletes an arbitrary tree as root. The committed runner is filtered to a single test, and a committed result file records it failing along with a test password and local paths.

#### J12. Repository hygiene — **Low / Info** · Verified
**Where:** [.gitignore](../.gitignore), [launchSettings.json](../Rdn.CLI/Properties/launchSettings.json)
**What:** Apart from J1, a sweep of the tree and its history found only test fixtures and examples, no other real secret. `.gitignore` does not cover keystores, certificates, wallet files (`*.uwa`), settings profiles, `.env`, IDE folders or test results. Tracked by accident: IDE state folders (including an assistant's diff state with file contents), a 50 MB test file, a result file, old project trees. Launch profiles contain public addresses and ports of development nodes and a placeholder access key; test documents link to internal sheets and pages.

#### J13. Build settings — **Info** · Verified
**Where:** [Uccs.Net.csproj:5](../Net/Uccs.Net.csproj#L5)
**What:** Code analyzers are switched off almost everywhere, nullable checking is off in the core projects, and arithmetic overflow checking is not enabled, so consensus arithmetic wraps silently. `AllowUnsafeBlocks` is set where no unsafe code exists. Around forty production code paths behave differently when a debugger is attached.

#### J14. Mobile app (`Umc`) — **Info** · Likely
**What:** It registers only mock services and has no network code, so it handles no real keys today. It stores a 4-digit PIN in secure storage with no attempt limit, skips biometrics in Debug builds, and allows device backup.

#### J15. Vote hash uses one byte of the zone number — **Info** · Verified
**Where:** [Vote.cs:109](../Net/Mcv/Vote.cs#L109)
**What:** `(byte)Mcv.Net.Zone` keeps only the low byte of a 16-bit zone id. Today's zones still differ in that byte, but two zones whose ids are 256 apart would accept each other's votes if they share members.

---

## 4. Questions for the team

The answers would change the severity or the wording of several findings.

**Deployment and trust model**
1. Which zones are reachable from the public internet today, and does anything of value live on them yet? (Both `Rdn.Main` and `Fair.Main` are `null`, so I assumed the public network is the Test zone.)
2. Are the vault, Nexus and node HTTP APIs meant to be called by web pages (the Fair UI does), by other local programs, or only by Ultranet's own processes? Is `PublicIP` ever set in practice?
3. Do consensus members also run the public Fair web API in the same process (H1, H2)?
4. Do end users run a full Rdn node or the default light configuration that asks a single peer for everything (C8, F4)?
5. Should the Nexus on an end user's machine listen for ICCP on all network interfaces (D1)?

**Intent of specific code**
6. Is `OverrideAuthentication` used by anything outside this repository? Is `Batch` needed on the vault server (A1, A2)?
7. Is ignoring the answer of the authorization dialog an oversight, or does something rely on it (A3)?
8. Is the genesis-key exemption in `UtilityTransfer` a development faucet (B1)? Should that key be usable for accounts at all?
9. Is "open an address → download and run" meant to happen without a consent step, and what is the intended trust model for package publishers (A4, F4)? Which program is the `iccp:` link handler supposed to start (J2)?
10. Is `Dependency.Name` meant to pin a dependency to an exact release (F12)? Should locally built releases stay private until published (F13)?
11. May non-image files be stored on the Fair chain and served by the API (I1)?

**Design decisions still open**
12. Fee model: should an exhausted energy rating reject a transaction? Should proof of work be mandatory for accounts without bandwidth (B10)?
13. Membership: is "anyone who pays the fee" the intended admission rule, and what is the intended quorum rule (B11, B13)?
14. Synchronisation: are signed rounds or checkpoints planned (C1)? Is transport encryption planned (C7)?
15. Should the vault receive the transaction itself rather than its hash, and who should decide when manual approval is required (A8)?
16. Inter-chain transfers: what is the plan for authenticating them (D2)?

**Housekeeping**
17. Was any APK signed with the committed keystore ever distributed (J1)?
18. Where is the private key of the shared genesis account kept, and will each zone get its own (J5)?
19. Was `Nas.sol` deployed, who holds its owner key, and is it still part of the design (J9)?
20. Is a move to .NET 10 planned (J8)?

**Chain rules**
21. Are the data files a node reads during validation and confirmation (reserved names, author ranks) guaranteed to be identical on every node, and how (E4, G9)?
22. Is the first transaction of a new account meant to be accepted without any signature (B8)?
23. Is "half" the intended threshold for moderator and publisher votes with even numbers (G6)? Should review storage be charged to the author or to the store (G10)? Should an author be able to change its own limits in a store (G12)?
24. Is the web UI always served by the same host that serves the API (I3)?
25. Are the moderation and user routes of the web API meant to be public (H10)? Will the web port be exposed directly or behind a reverse proxy that provides TLS, limits and headers (H4, H9)?
26. Which maximum sizes and counts does the chain guarantee (avatars, product fields, files, open proposals, categories per store)? The web API has no caps of its own and relies on them (H3, H5).

---

## 5. Coverage and limits

**How the review was done.** I read the engine and the daemon myself: `CS/` (serialisation, JSON server and client, settings, CLI base), `Net/` (peering, consensus, cryptography, node APIs, ICCP, local pipe) and the vault, wallet and API parts of `Nexus/`. Six parallel reviewers covered the outer modules (Rdn chain logic, release distribution and packages, Fair chain logic, Fair web API, Fair web UI, desktop hosts and repository hygiene). I then re-read the code behind their main findings before including them; findings I could not re-check myself are marked *Likely*.

**Limits.**
- Nothing was executed. No exploit was built or tried, no fuzzing, no dependency audit against online databases.
- The native C++ projects were out of scope. The bundled data files (`Fair/Data`, `Rdn/Data`), the `.docx` whitepapers, images and most of the mobile app's views were not read. Individual test cases were only sampled.
- One reviewer (Fair web API) was stopped part-way by an automated safety filter after reporting H1 and H2. The rest of that area was redone with a narrower, fix-oriented brief (H3–H11); that pass describes weaknesses and fixes but was not asked to work out how each could be abused.
- Roughly two thirds of the web UI's ~1,000 files were covered by targeted search rather than read line by line.
- Severity reflects the code as written, not how a given network is actually deployed. Several ratings depend on the questions above.

**What looked sound.** Transaction and vote hashes include the zone and the net address, so signatures do not carry over between nets. Signatures are BIP-340 Schnorr over secp256k1 through an established library, and private keys are generated with the system random generator. Wallet passwords are stretched with Argon2id using strong parameters, and salts and IVs come from the system random generator. The Fair web API is read-only and never serves user content as HTML. The web UI has no HTML-injection sinks.

