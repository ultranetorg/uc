import { TFunction } from "i18next"

import { ListRow, TableColumn } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "name", label: t("name") },
  { accessor: "expirationDay", label: t("expiration") },
  { accessor: "lastWinner", label: t("lastWinner") },
  { accessor: "lastBid", label: t("lastBid"), type: "currency" },
  { accessor: "lastBidDay", label: t("lastBidTime") },
  { accessor: "highestBidBy", label: t("highestBidBy") },
  { accessor: "highestBid", label: t("highestBid"), type: "currency" },
]

export const getTableColumns = (
  t: TFunction<string, undefined, string>,
  hasExtendedColumns?: boolean,
): TableColumn[] => [
  { accessor: "bidBy", label: t("bidBy"), className: "w-5/12" },
  { accessor: "bid", label: t("bid"), className: "w-4/12 text-right" },
  ...(hasExtendedColumns ? [{ accessor: "bidDay", label: t("bidTime"), className: "w-3/12 text-right" }] : []),
]
