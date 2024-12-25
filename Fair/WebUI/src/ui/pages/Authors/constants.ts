import { TFunction } from "i18next"

import { ListRow } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "name", label: t("name") },
  { accessor: "owner", label: t("owner") },
  { accessor: "expirationDay", label: t("expiration") },
  { accessor: "comOwner", label: t("comOwner"), type: "account" },
  { accessor: "orgOwner", label: t("orgOwner"), type: "account" },
  { accessor: "netOwner", label: t("netOwner"), type: "account" },
  { accessor: "firstBidDay", label: t("firstBidDay") },
  { accessor: "lastWinner", label: t("lastWinner") },
  { accessor: "lastBid", label: t("lastBid"), type: "currency" },
  { accessor: "lastBidDay", label: t("lastBidDay") },
  { accessor: "spaceReserved", label: t("spaceReserved") },
  { accessor: "spaceUsed", label: t("spaceUsed") },
]
