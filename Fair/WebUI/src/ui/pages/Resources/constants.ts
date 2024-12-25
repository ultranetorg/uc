import { TFunction } from "i18next"

import { ListRow, TableColumn } from "ui/components"

export const getListRows = (t: TFunction<string, undefined, string>): ListRow[] => [
  { accessor: "address.author", label: t("author"), type: "author" },
  { accessor: "address.resource", label: t("resource"), type: "text_copyable" },
  { accessor: "id", label: t("id") },
  { accessor: "flags", label: t("flags"), type: "enums" },
  { accessor: "data.type", label: t("dataType") },
  { accessor: "data.value", label: t("dataValue") },
  { accessor: "data.length", label: t("dataLength") },
  { accessor: "updated", label: t("updated") },
]

export const getTableColumns = (t: TFunction<string, undefined, string>): TableColumn[] => [
  { accessor: "address.author", label: t("address"), className: "w-6/12 text-left", type: "address" },
  { accessor: "flags", label: t("flags"), className: "w-6/12 text-left" },
]
