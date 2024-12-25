import { OperationType } from "types"
import { ListRow } from "ui/components"

const baseRows: ListRow[] = [
  { accessor: "$type", label: "Operation type" },
  { accessor: "id", label: "Id" },
  { accessor: "signer", label: "Signer" },
  { accessor: "transactionId", label: "Transaction Id" },
]

const analysisResultUpdationRows: ListRow[] = [
  ...baseRows,
  { accessor: "resource.author", label: "Author" },
  { accessor: "resource.resource", label: "Resource" },
  { accessor: "release", label: "Release" },
  { accessor: "consil", label: "Consil" },
  { accessor: "result", label: "Result" },
]

const authorBidRows: ListRow[] = [
  ...baseRows,
  { accessor: "author", label: "Author" },
  { accessor: "bid", label: "Bid", type: "currency" },
  { accessor: "tld", label: "Tld" },
]

const authorMigrationRows: ListRow[] = [
  ...baseRows,
  { accessor: "author", label: "Author" },
  { accessor: "tld", label: "Tld" },
  { accessor: "rankCheck", label: "Rank Check" },
]

const authorRegistrationBidRows: ListRow[] = [
  ...baseRows,
  { accessor: "name", label: "Name" },
  { accessor: "years", label: "Years" },
]

const authorTransferRows: ListRow[] = [
  ...baseRows,
  { accessor: "author", label: "Author" },
  { accessor: "to", label: "To" },
]

const candidacyDeclarationRows: ListRow[] = [
  ...baseRows,
  { accessor: "bail", label: "Bail", type: "currency" },
  { accessor: "baseRdcIPs", label: "Base RDC IPs", type: "string_array" },
  { accessor: "seedHubRdcIPs", label: "Seed Hub RDC IPs", type: "string_array" },
]

const emissionRows: ListRow[] = [
  ...baseRows,
  { accessor: "eid", label: "Emission Id" },
  { accessor: "wei", label: "Wei", type: "currency" },
]

const resourceCreationRows: ListRow[] = [
  ...baseRows,
  { accessor: "resource.author", label: "Author", type: "author" },
  { accessor: "resource.resource", label: "Resource" },
  { accessor: "flags", label: "Flags" },
  { accessor: "data.type", label: "Data Type", type: "enums" },
  { accessor: "data.value", label: "Data Value" },
  { accessor: "data.length", label: "Data Length" },
  { accessor: "parent", label: "Parent" },
]

const resourceDeletionRows: ListRow[] = [
  ...baseRows,
  { accessor: "resource.author", label: "Author", type: "author" },
  { accessor: "resource.resource", label: "Resource" },
]

const resourceLinkCreationRows: ListRow[] = [
  ...baseRows,
  { accessor: "source.author", label: "Author", type: "author" },
  { accessor: "source.resource", label: "Resource" },
  { accessor: "destination.author", label: "To Author", type: "author" },
  { accessor: "destination.resource", label: "To Resource" },
]

const resourceLinkDeletionRows: ListRow[] = [
  ...baseRows,
  { accessor: "source.author", label: "Author", type: "author" },
  { accessor: "source.resource", label: "Resource" },
  { accessor: "destination.author", label: "To Author", type: "author" },
  { accessor: "destination.resource", label: "To Resource" },
]

const resourceUpdationRows: ListRow[] = [
  ...baseRows,
  { accessor: "resource.author", label: "Author", type: "author" },
  { accessor: "resource.resource", label: "Resource" },
  { accessor: "changes", label: "Changes" },
  { accessor: "flags", label: "Flags" },
  { accessor: "data.type", label: "Data Type", type: "enums" },
  { accessor: "data.value", label: "Data Value" },
  { accessor: "data.length", label: "Data Length" },
  { accessor: "parent", label: "Parent" },
]

const untTransferRows: ListRow[] = [
  ...baseRows,
  { accessor: "to", label: "To" },
  { accessor: "amount", label: "Amount", type: "currency" },
]

export const rowsToOperationTypeMap: Map<OperationType, ListRow[]> = new Map([
  ["analysisResultUpdation", analysisResultUpdationRows],
  ["authorBid", authorBidRows],
  ["authorMigration", authorMigrationRows],
  ["authorRegistration", authorRegistrationBidRows],
  ["authorTransfer", authorTransferRows],
  ["candidacyDeclaration", candidacyDeclarationRows],
  ["emission", emissionRows],
  ["resourceCreation", resourceCreationRows],
  ["resourceDeletion", resourceDeletionRows],
  ["resourceLinkCreation", resourceLinkCreationRows],
  ["resourceLinkDeletion", resourceLinkDeletionRows],
  ["resourceUpdation", resourceUpdationRows],
  ["untTransfer", untTransferRows],
])
