import { TFunction } from "i18next"

import { ListRow } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "id", label: t("id") },
  { accessor: "roundId", label: t("roundId") },
  { accessor: "signer", label: t("signer") },
  { accessor: "nid", label: t("nid") },
  { accessor: "fee", label: t("fee"), type: "currency" },
  { accessor: "tag", label: t("tag") },
]
