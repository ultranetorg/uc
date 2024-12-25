import { TFunction } from "i18next"

import { ListRow } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "address", label: t("address") },
  { accessor: "balance", label: t("balance"), type: "currency" },
  // { accessor: "bail", label: t("bail"), type: "currency" },
  { accessor: "bailStatus", label: t("bailStatus") },
  { accessor: "authors", label: t("authors") },
  { accessor: "lastTransactionNid", label: t("lastTransactionNid") },
  { accessor: "lastEmissionId", label: t("lastEmissionId") },
  // { accessor: "candidacyDeclarationRid", label: t("candidacyDeclarationRid") },
  { accessor: "averageUptime", label: t("averageUptime") },
]

// "lastTransactionNid": 10,
// "lastEmissionId": 0,
// "candidacyDeclarationRid": 10,
