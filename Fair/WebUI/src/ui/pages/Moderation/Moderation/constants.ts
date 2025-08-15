import { TFunction } from "i18next"
import { TableColumn } from "ui/components"

export const getCommonColumns = (t: TFunction): TableColumn[] => [
  { accessor: "lastsFor", label: t("common:lastsFor"), type: "lasts-for", className: "w-[8%]" },
  { accessor: "votes", label: t("common:votes"), type: "votes", className: "w-[8%]" },
  { accessor: "anbb", label: t("common:anbb"), type: "anbb", title: t("common:anbbFull"), className: "w-[9%]" },
]
