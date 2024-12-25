import { CardListRow } from "ui/components"

export const costRows: CardListRow[] = [
  { accessor: "rentBytePerDay", label: "Rent Byte per Day", type: "currency" },
  { accessor: "exeunit", label: "Execution Unit (minimal)", type: "currency" },
  { accessor: "rentAccount", label: "Rent Account", type: "currency" },
  { accessor: "rentAuthor", label: "Rent Author", display: "column" },
  { accessor: "rentResource", label: "Rent Resource", display: "column" },
  { accessor: "rentResourceForever", label: "Rent Resource Forever", type: "currency" },
  { accessor: "rentResourceData", label: "Rent Resource Data", display: "column" },
  { accessor: "rentResourceDataForever", label: "Rent Resource Data Forever", type: "currency" },
]

export const stateRows: CardListRow[] = [
  { accessor: "lastRoundId", label: "Last Round Id" },
  { accessor: "emission", label: "Emission", type: "currency" },
  { accessor: "emissionInPct", label: "Emission in %" },
  { accessor: "day", label: "Current Day" },
  { accessor: "baseSize", label: "Base Size" },
  { accessor: "minimalMembershipBail", label: "Minimal Membership Bail", type: "currency" },
]

export const statisticsRows: CardListRow[] = [
  { accessor: "rounds", label: "Rounds" },
  { accessor: "transactions", label: "Transactions" },
  { accessor: "operations", label: "Operations" },
  { accessor: "members", label: "Members" },
  { accessor: "authorsNormal", label: "Authors Normal" },
  { accessor: "authorsAuctioned", label: "Authors Auctioned" },
  { accessor: "resources", label: "Resources" },
  { accessor: "analysesResults", label: "Analysis Results" },
]

export const performanceRows: CardListRow[] = [
  { accessor: "operationsPerSecond", label: "Operations Per Second" },
  { accessor: "transactionsPerSecond", label: "Transactions Per Second" },
]
