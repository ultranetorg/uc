import { TFunction } from "i18next"
import { TableColumn } from "ui/components"

export const getCommonColumns = (t: TFunction): TableColumn[] => [
  { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for" },
  { accessor: "votes", label: t("common:votes"), type: "votes" },
  { accessor: "anbb", label: t("common:anbb"), type: "anbb", title: t("common:anbbFull") },
]
