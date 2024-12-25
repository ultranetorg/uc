import { TFunction } from "i18next"

import { TableColumn } from "ui/components"

export const getTableColumns = (
  t: TFunction<string, undefined, string>,
  hasExtendedColumns?: boolean,
): TableColumn[] => [
  { accessor: "name", label: t("author"), className: "text-center" },
  { accessor: "expirationDay", label: t("expiration"), className: "text-right" },
  { accessor: "lastBid", label: t("lastBid"), className: "text-right" },
  ...(hasExtendedColumns ? [{ accessor: "lastBidDay", label: t("lastBidDay"), className: "text-right" }] : []),
]
