# CLAUDE.md

Ultranet "UC" repo: a custom blockchain platform (C#, .NET 9). It has one reusable consensus engine and two application chains on top of it (Rdn, Fair), plus a local host daemon (Nexus) and web front ends.

## Project graph

```
CS (Uccs)  →  Net (Uccs.Net)  →  Nexus  →  Nexus.CLI / Nexus.Windows / Rdn.CLI / Tests/CS
                    └──────────→  Fair/cs (+ Web)  →  Fair.CLI / Fair.Windows (+ Mcv.FUI)
```

There is no root C# solution. Build each project with `dotnet build <path>.csproj`. `TestAutomation/TestAutomation.sln` exists for the e2e tests.

## Projects

- **`CS/`** (`Uccs`): base utilities. Binary `Reader`/`Writer`, the **Xon** config/serialization format (`CS/Xon`), `Log`, `Flow` (cancellation + logging context), the CLI framework (`Cli`/`Command`), `JsonClient`/`JsonServer`.
- **`Net/`** (`Uccs.Net`): the generic ledger engine.
  - `Net/Node`: TCP peering (`Node`, `Peer`, `TcpPeering`, `HomoPeering`). **Ppc** = peer-to-peer call (request types named `*Ppc`).
  - `Net/Mcv`: **MCV = Mutual Chain Voting** consensus. At most 21 voting `Member`s (joined via `CandidacyDeclaration`), `Round`s and `Vote`s, `Transaction`s carrying `Operation`s, `Execution`, and `Table`s persisted in RocksDB.
  - `Net/Mcv/Net.cs`: `McvNet` network descriptors, zones (Simulation, Virtual, Developer0, Test, TA) and economy limits (`MembersMaximum`, `OperationsPerRoundMaximum`, …).
  - `Net/Mcv/McvNodeSettings.cs`: node settings, e.g. `ManualApprovalRequired`.
  - `Net/Iccp` (inter-chain protocol), `Net/Lcp.cs` (local connections), `Net/Api.cs` (JSON HTTP API base), `Net/VaultApi.cs`, `Net/CLI`.
- **`Nexus/`**: the local daemon. It holds the `Vault` (keys/wallets), `PackageHub` (software packages), Iccp, and **embeds the Rdn chain**.
  - **Rdn** (domains, resources, releases/seeding) lives in `Nexus/Rdn/`. The top-level `Rdn/` folder only holds domain-name data lists (`Rdn/Data/{com,org,...}`), which are large.
  - Hosts: `Nexus.CLI` (`nexus`), `Nexus.Windows` (WinForms), `Rdn.CLI` (`rdn`).
- **`Fair/cs`**: the second chain, a decentralized catalog/marketplace. Domains: Store, Category, Product, Publication, Review, Author, User, File, Search, Proposal/Survey (moderation/governance).
  - Each layer subclasses the Net base class: `Fair : McvNet`, `FairMcv : Mcv`, `FairNode : McvNode`, `FairRound : Round`, `FairExecution : Execution`, `FairOperation`, `FairPpc<R> : McvPpc<R>`, `FairApiServer : McvApiServer`.
  - The ASP.NET Core REST API is in `Fair/cs/WebServer/` (Controllers, including `Moderation/`, plus Services/Models/Validators). Sample requests are in `WebServer*.http`.
  - Hosts: `Fair.CLI`, `Fair.Windows` (uses `Mcv.FUI`, a generic WinForms node admin UI).
- **`Fair/WebUI`**: the Fair front end, built with React 19, Vite, TypeScript, React Router 7, TanStack Query, Tailwind 4 and i18next. **There is no test runner.**
  - Scripts: `dev`, `build` (`prebuild` runs `sync`), `lint`, `format`.
  - `npm run sync` (`tools/sync-with-node.ts`) copies C# constants (e.g. operation error names) into TS. Re-run it after changing those in C#.
  - It has its own `Fair/WebUI/CLAUDE.md`. Follow it when working there.
- **`Web/`**: shared ASP.NET Core helpers (middlewares, filters, pagination) used by Fair's WebServer.
- **`Web.UI/`**: shared React component library (Storybook, yarn). Not used by Fair/WebUI.

## Tests

- `Tests/CS`: xunit v3 unit tests (keys, Bytes, Xon, serialization, EC, addresses).
- `TestAutomation/`: custom e2e runner, not xunit.
  - `Program.cs` reflects over `TestBase` subclasses and runs `Init()` then `Logic()`. Tests run on `Zone.TA` with in-process Nexus nodes.
  - Covers Rdn, Fair and the wallet. Writes JUnit XML results.
  - Needs env vars `UO` (root uo folder) and `UO_Tmp`. Run it via `run.sh`.

## Legacy / side folders

- `Umc/`: MAUI mobile app on mock data. Its project references are stale, so it doesn't build as is.
- `Demo/`: WinForms sample app.
- `Nas.sol`: Ethereum activation contract (burn ETH; zone seed-node registry).
- Root `*.docx`: whitepapers (MCV, RIL, UOSAMI, DDOSATP).
- Native C++ UOS side (`Uos/`, `C/`, `Shell/`, `Vwm/`, `Experimental/`, `Image.DevIL/`, `Libs/`): a separate 3D shell. Out of scope unless explicitly asked.
