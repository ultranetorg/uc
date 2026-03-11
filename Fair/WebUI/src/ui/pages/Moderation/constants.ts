import { TFunction } from "i18next"
import { TableColumn } from "ui/components"

export const getVotingColumns = (t: TFunction): TableColumn[] => [
  { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for" },
  { accessor: "votes", label: t("common:votes"), type: "votes" },
  { accessor: "nabb", label: t("common:nabb"), type: "nabb", title: t("common:nabbFull") },
]
